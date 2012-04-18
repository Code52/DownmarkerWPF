using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Awesomium.Core;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using MarkPad.Extensions;
using MarkPad.Framework;
using MarkPad.Framework.Events;
using MarkPad.MarkPadExtensions;
using MarkPad.Services.Interfaces;
using MarkPad.Services.MarkPadExtensions;
using MarkPad.Services.Settings;
using MarkPad.XAML;

namespace MarkPad.Document
{
    public partial class DocumentView : IHandle<SettingsChangedEvent>
    {
        private const int NumSpaces = 4;
        private const string Spaces = "    ";
		private const double ZoomDelta = 0.1;
		private const string LOCAL_REQUEST_URL_BASE = "local://base_request.html/";

        private ScrollViewer documentScrollViewer;
		private readonly IList<IDocumentViewExtension> extensions = new List<IDocumentViewExtension>();
		private readonly ISettingsProvider settingsProvider;

		MarkPadSettings settings;

		public DocumentView(ISettingsProvider settingsProvider)
        {
			this.settingsProvider = settingsProvider;

            InitializeComponent();
            
			Loaded += DocumentViewLoaded;
            wb.Loaded += WbLoaded;
            wb.OpenExternalLink += WebControl_LinkClicked;
			wb.ResourceRequest += WebControl_ResourceRequest;
            SizeChanged += DocumentViewSizeChanged;
            Editor.TextArea.SelectionChanged += SelectionChanged;
            Editor.PreviewMouseLeftButtonUp += HandleMouseUp;
			Editor.MouseWheel += HandleEditorMouseWheel;
			Editor.MouseMove += HandleEditorMouseMove;
			Editor.PreviewMouseLeftButtonDown += HandleEditorPreviewMouseLeftButtonDown;

			settings = this.settingsProvider.GetSettings<MarkPadSettings>();

			ApplyExtensions();

            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleBold, (x, y) => ToggleBold(), CanEditDocument));
            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleItalic, (x, y) => ToggleItalic(), CanEditDocument));
            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleCode, (x, y) => ToggleCode(), CanEditDocument));
            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleCodeBlock, (x, y) => ToggleCodeBlock(), CanEditDocument));
			CommandBindings.Add(new CommandBinding(FormattingCommands.SetHyperlink, (x, y) => SetHyperlink(), CanEditDocument));
			CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomIn, (x, y) => ZoomIn()));
			CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomOut, (x, y) => ZoomOut()));
			CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomReset, (x, y) => ZoomReset()));

			Editor.MouseMove += (s, e) => e.Handled = true;
			ZoomSlider.ValueChanged += (sender, e) => ApplyZoom();
        }

		private void ApplyZoom()
		{
			Editor.TextArea.TextView.Redraw();

			var zoom = ZoomSlider.Value;
			
			var fontSize = GetFontSize() * zoom;

			Editor.FontSize = fontSize;
			wb.Zoom = GetZoomLevel(fontSize);
		}

		private void ZoomIn()
		{
			AdjustZoom(ZoomDelta);
		}

		private void ZoomOut()
		{
			AdjustZoom(-ZoomDelta);
		}

		private void AdjustZoom(double delta)
		{
			var newZoom = ZoomSlider.Value + delta;

			if (newZoom < ZoomSlider.Minimum) newZoom = ZoomSlider.Minimum;
			if (newZoom > ZoomSlider.Maximum) newZoom = ZoomSlider.Maximum;

			ZoomSlider.Value = newZoom;
		}

		private void ZoomReset()
		{
			ZoomSlider.Value = 1;
		}

		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			base.OnPreviewMouseWheel(e);
			
			if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) return;

			e.Handled = true;

			if (e.Delta > 0) ZoomIn();
			else ZoomOut();
		}

		private void ApplyFont()
		{
			Editor.FontFamily = GetFontFamily();
		}

		private void ApplyExtensions()
		{
			var allExtensions = MarkPadExtensionsProvider.Extensions.OfType<IDocumentViewExtension>().ToList();
			var extensionsToAdd = allExtensions.Except(extensions).ToList();
			var extensionsToRemove = extensions.Except(allExtensions).ToList();

			foreach (var extension in extensionsToAdd)
			{
				extension.ConnectToDocumentView(this);
				extensions.Add(extension);
			}

			foreach (var extension in extensionsToRemove)
			{
				extension.DisconnectFromDocumentView(this);
				extensions.Remove(extension);
			}
		}

		void WebControl_LinkClicked(object sender, OpenExternalLinkEventArgs e)
		{
			// Although all links have "target='_blank'" added (see ParsedDocument.ToHtml()), they go through this first
			// unless the url is local (a bug in Awesomium) in which case this event isn't triggered, and the "target='_blank'"
			// takes over to avoid crashing the preview. Local resource requests where the resource doesn't exist are thrown
			// away. See WebControl_ResourceRequest().

			string filename = e.Url;
			if (e.Url.StartsWith(LOCAL_REQUEST_URL_BASE))
			{
				filename = GetResourceFilename(e.Url.Replace(LOCAL_REQUEST_URL_BASE, "")) ?? "";
				if (!File.Exists(filename)) return;
			}

			if (string.IsNullOrWhiteSpace(filename)) return;

			Process.Start(filename);
		}

		ResourceResponse WebControl_ResourceRequest(object o, ResourceRequestEventArgs e)
		{
			// This tries to get a local resource. If there is no local resource null is returned by GetLocalResource, which
			// triggers the default handler, which should respect the "target='_blank'" attribute added
			// in ParsedDocument.ToHtml(), thus avoiding a bug in Awesomium where trying to navigate to a
			// local resource fails when showing an in-memory file (https://github.com/Code52/DownmarkerWPF/pull/208)

			// What works:
			//	- resource requests for remote resources (like <link href="http://somecdn.../jquery.js"/>)
			//	- resource requests for local resources that exist relative to filename of the file (like <img src="images/logo.png"/>)
			//	- clicking links for remote resources (like [Google](http://www.google.com))
			//	- clicking links for local resources which don't exist (eg [test](test)) does nothing (WebControl_LinkClicked checks for existence)
			// What fails:
			//	- clicking links for local resources where the resource exists (like [test](images/logo.png))
			//		- This _sometimes_ opens the resource in the preview pane, and sometimes opens the resource 
			//		using Process.Start (WebControl_LinkClicked gets triggered). The behaviour seems stochastic.
			//	- alt text for images where the image resource is not found

			if (e.Request.Url.StartsWith(LOCAL_REQUEST_URL_BASE)) return GetLocalResource(e.Request.Url.Replace(LOCAL_REQUEST_URL_BASE, ""));

			// If the request wasn't local, return null to let the usual handler load the url from the network			
			return null;
		}
		ResourceResponse GetLocalResource(string url)
		{
			if (string.IsNullOrWhiteSpace(url)) return null;

			var resourceFilename = GetResourceFilename(url);
			if (!File.Exists(resourceFilename)) return null;

			return new ResourceResponse(resourceFilename);
		}
		public string GetResourceFilename(string url)
		{
			var vm = DataContext as DocumentViewModel;
			if (vm == null) return null;
			if (string.IsNullOrEmpty(vm.FileName)) return null;

			var resourceFilename = Path.Combine(Path.GetDirectoryName(vm.FileName), url);
			return resourceFilename;
		}

        void DocumentViewSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Hide web browser when the window is too small for it to make much sense
            webBrowserColumn.MaxWidth = e.NewSize.Width <= 350 ? 0 : double.MaxValue;
        }

        /// <summary>
        /// Get the font size that was set in the settings.
        /// </summary>
        /// <returns>Font size.</returns>
        private int GetFontSize()
        {
            return Constants.FONT_SIZE_ENUM_ADJUSTMENT + (int)settings.FontSize;
        }

		private FontFamily GetFontFamily()
		{
			var configuredSource = settings.FontFamily;
			var fontFamily = FontHelpers.TryGetFontFamilyFromStack(configuredSource, "Segoe UI", "Arial");
			if (fontFamily == null) throw new Exception("Cannot find configured font family or fallback fonts");
			return fontFamily;
		}

        /// <summary>
        /// Turn the font size into a zoom level for the browser.
        /// </summary>
        /// <returns></returns>
        private static int GetZoomLevel(double fontSize)
        {
            // The default font size 12 corresponds to 100 (which maps to 0 here); for an increment of 1, we add 50/6 to the number.
            // For 18 we end up with 150, which looks really fine. TODO: Feel free to try to further outline this, but this is a good start.
            var zoom = 100.0 + (fontSize - Constants.FONT_SIZE_ENUM_ADJUSTMENT) * 40.0 / 6.0;

            // Limit the zoom by the limits of Awesomium.NET.
            if (zoom < 50) zoom = 50;
            if (zoom > 500) zoom = 500;
            return (int)zoom;
        }

        private void WbProcentualZoom()
        {
			ApplyZoom();
            wb.ExecuteJavascript("window.scrollTo(0," + documentScrollViewer.VerticalOffset / (documentScrollViewer.ExtentHeight - documentScrollViewer.ViewportHeight) + " * (document.body.scrollHeight - document.body.clientHeight));");
        }

        void WbLoaded(object sender, RoutedEventArgs e)
        {
            WbProcentualZoom();
        }

        private void DocumentViewLoaded(object sender, RoutedEventArgs e)
        {
            using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream("MarkPad.Syntax.Markdown.xshd"))
            using (var reader = new XmlTextReader(stream))
            {
                Editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            documentScrollViewer = Editor.FindVisualChild<ScrollViewer>();

            var viewModel = ((DocumentViewModel)DataContext);
            if (documentScrollViewer != null)
            {
                documentScrollViewer.ScrollChanged += (i, j) => WbProcentualZoom();
                viewModel.Document.TextChanged += (i, j) =>
                    {
                        wb.LoadCompleted += (k, l) => WbProcentualZoom();
                    };
            }

            // AvalonEdit hijacks Ctrl+I. We need to free that mutha up
            var editCommandBindings = Editor.TextArea.DefaultInputHandler.Editing.CommandBindings;

            editCommandBindings
                .FirstOrDefault(b => b.Command == AvalonEditCommands.IndentSelection)
                .ExecuteSafely(b => editCommandBindings.Remove(b));

            Editor.Focus();
        }

        internal void ToggleBold()
        {
            var selectedText = GetSelectedText();
            if (string.IsNullOrWhiteSpace(selectedText)) return;

            Editor.SelectedText = selectedText.ToggleBold(!selectedText.IsBold());
        }

        internal void ToggleItalic()
        {
            var selectedText = GetSelectedText();
            if (string.IsNullOrWhiteSpace(selectedText)) return;

            Editor.SelectedText = selectedText.ToggleItalic(!selectedText.IsItalic());
        }

        internal void ToggleCode()
        {
            if (Editor.SelectedText.Contains(Environment.NewLine))
                ToggleCodeBlock();
            else
            {
                var selectedText = GetSelectedText();
                if (string.IsNullOrWhiteSpace(selectedText)) return;

                Editor.SelectedText = selectedText.ToggleCode(!selectedText.IsCode());
            }
        }

        private string GetSelectedText()
        {
            var textArea = Editor.TextArea;
            // What would you do if the selected text is empty? I vote: Nothing.
            if (textArea.Selection.IsEmpty)
                return null;

            return textArea.Selection.GetText(textArea.Document);
        }

        private void ToggleCodeBlock()
        {
            var lines = Editor.SelectedText.Split(Environment.NewLine.ToCharArray());
            if (lines[0].Length > 4)
            {
                if (lines[0].Substring(0, 4) == Spaces)
                {
                    Editor.SelectedText = Editor.SelectedText.Replace((Environment.NewLine + Spaces), Environment.NewLine);

                    // remember the first line
                    if (Editor.SelectedText.Length >= NumSpaces)
                    {
                        var firstFour = Editor.SelectedText.Substring(0, NumSpaces);
                        var rest = Editor.SelectedText.Substring(NumSpaces);

                        Editor.SelectedText = firstFour.Replace(Spaces, string.Empty) + rest;
                    }
                    return;
                }
            }

            Editor.SelectedText = Spaces + Editor.SelectedText.Replace(Environment.NewLine, Environment.NewLine + Spaces);
        }

        internal void SetHyperlink()
        {
            var textArea = Editor.TextArea;
            if (textArea.Selection.IsEmpty)
                return;

            var selectedText = textArea.Selection.GetText(textArea.Document);

            //  Check if the selected text already is a link...
            string text = selectedText, url = string.Empty;
            var match = Regex.Match(selectedText, @"\[(?<text>(?:[^\\]|\\.)+)\]\((?<url>[^)]+)\)");
            if (match.Success)
            {
                text = match.Groups["text"].Value;
                url = match.Groups["url"].Value;
            }
            var hyperlink = new MarkPadHyperlink(text, url);

            (DataContext as DocumentViewModel)
                .ExecuteSafely(vm =>
                                   {
                                       hyperlink = vm.GetHyperlink(hyperlink);
                                       if (hyperlink != null)
                                       {
                                           textArea.Selection.ReplaceSelectionWithText(textArea,
                                               string.Format("[{0}]({1})", hyperlink.Text, hyperlink.Url));
                                       }
                                   });
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            if (Editor.TextArea.Selection.IsEmpty)
                floatingToolBar.Hide();
        }

        private void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
			if (!settings.FloatingToolBarEnabled)
				return;

			if (Editor.TextArea.Selection.IsEmpty)
				floatingToolBar.Hide();
			else
				ShowFloatingToolBar();
		}

		void HandleEditorMouseMove(object sender, MouseEventArgs e)
		{
			// Bail out if tool bar is disabled, if there is no selection, or if the toolbar is already open
			if (!settings.FloatingToolBarEnabled) return;
			if (string.IsNullOrEmpty(Editor.SelectedText)) return;
			if (floatingToolBar.IsOpen) return;
			if (e.LeftButton == MouseButtonState.Pressed) return;
			
			// Bail out if the mouse isn't over the editor
			var editorPosition = Editor.GetPositionFromPoint(e.GetPosition(Editor));
			if (!editorPosition.HasValue) return;
			
			// Bail out if the mouse isn't over a selection
			var offset = Editor.Document.GetOffset(editorPosition.Value.Line, editorPosition.Value.Column);
			if (offset < Editor.SelectionStart) return;
			if (offset > Editor.SelectionStart + Editor.SelectionLength) return;

			ShowFloatingToolBar();
		}

		void HandleEditorPreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			if (!floatingToolBar.IsOpen) return;
			floatingToolBar.Hide();
		}

		private void ShowFloatingToolBar()
		{
			// Find the screen position of the start of the selection
			var selectionStartLocation = Editor.Document.GetLocation(Editor.SelectionStart);
			var selectionStartPosition = new TextViewPosition(selectionStartLocation);
			var selectionStartPoint = Editor.TextArea.TextView.GetVisualPosition(selectionStartPosition, VisualYPosition.LineTop);

			var popupPoint = new Point(
				selectionStartPoint.X + 30,
				selectionStartPoint.Y - 35);

			floatingToolBar.Show(Editor, popupPoint);
		}

		void HandleEditorMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) return;
			ZoomSlider.Value += e.Delta * 0.1;
		}

        private void CanEditDocument(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Editor != null && Editor.TextArea != null && Editor.TextArea.Selection != null)
            {
                e.CanExecute = !Editor.TextArea.Selection.IsEmpty;
            }
        }

        void IHandle<SettingsChangedEvent>.Handle(SettingsChangedEvent message)
        {
			settings = settingsProvider.GetSettings<MarkPadSettings>();

			ApplyFont();
			ApplyZoom();
			ApplyExtensions();
        }

        private void EditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control || e.Key != Key.V) return;
            (DataContext as DocumentViewModel)
                .ExecuteSafely(d =>
                {
                    var siteContext = d.SiteContext;
                    var images = Clipboard.GetDataObject().GetImages();
                    if (images.Any() && siteContext != null)
                    {
                        var sb = new StringBuilder();
                        var textArea = Editor.TextArea;

                        foreach (var dataImage in images)
                        {
                            var relativePath = siteContext.SaveImage(dataImage.Bitmap);

                            sb.AppendLine(string.Format("![{0}](/{1})",
                                Path.GetFileNameWithoutExtension(relativePath), 
                                relativePath.TrimStart('/').Replace('\\', '/')));
                        }

                        textArea.Selection.ReplaceSelectionWithText(textArea, sb.ToString().Trim());
                        e.Handled = true;
                    }
                });
        }

        private void TreeViewMouseDoubleClick1(object sender, MouseButtonEventArgs e)
        {
             var selectedItem = siteFiles.SelectedItem as ISiteItem;

            if (selectedItem !=null)
            {
                (DataContext as DocumentViewModel)
                    .ExecuteSafely(d => d.SiteContext.OpenItem(selectedItem));
            }
        }
    }
}