using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Awesomium.Core;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MarkPad.Extensions;
using MarkPad.Framework;
using MarkPad.Framework.Events;
using MarkPad.Services.Settings;
using MarkPad.XAML;
using System.Windows.Media;
using MarkPad.Services.MarkPadExtensions;
using MarkPad.MarkPadExtensions;

namespace MarkPad.Document
{
    public partial class DocumentView : IHandle<SettingsChangedEvent>
    {
        private const int NumSpaces = 4;
        private const string Spaces = "    ";
		private const double ZOOM_IN_OUT_DELTA = 0.1;

        private ScrollViewer documentScrollViewer;
		private IList<IDocumentViewExtension> extensions = new List<IDocumentViewExtension>();
		private readonly ISettingsProvider settingsProvider;

		public DocumentView(ISettingsProvider settingsProvider)
        {
			this.settingsProvider = settingsProvider;
            InitializeComponent();
            Loaded += DocumentViewLoaded;
            wb.Loaded += WbLoaded;
            wb.OpenExternalLink += WebControl_LinkClicked;
            SizeChanged += DocumentViewSizeChanged;
            Editor.TextArea.SelectionChanged += SelectionChanged;
            Editor.PreviewMouseLeftButtonUp += HandleMouseUp;
			Editor.MouseWheel += HandleEditorMouseWheel;

			ApplyExtensions();

            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleBold, (x, y) => ToggleBold(), CanEditDocument));
            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleItalic, (x, y) => ToggleItalic(), CanEditDocument));
            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleCode, (x, y) => ToggleCode(), CanEditDocument));
            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleCodeBlock, (x, y) => ToggleCodeBlock(), CanEditDocument));
			CommandBindings.Add(new CommandBinding(FormattingCommands.SetHyperlink, (x, y) => SetHyperlink(), CanEditDocument));
			CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomIn, (x, y) => ZoomIn()));
			CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomOut, (x, y) => ZoomOut()));
			CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomReset, (x, y) => ZoomReset()));

			Editor.MouseMove += new MouseEventHandler((s, e) => e.Handled = true);
			
			ZoomSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>((sender, e) => ApplyZoom());
        }

		private void ApplyZoom()
		{
			Editor.TextArea.TextView.Redraw();

			var zoom = ZoomSlider.Value;
			
			var fontSize = (double)GetFontSize() * zoom;

			Editor.FontSize = fontSize;
			wb.Zoom = GetZoomLevel(fontSize);
		}

		private void ZoomIn()
		{
			AdjustZoom(ZOOM_IN_OUT_DELTA);
		}
		private void ZoomOut()
		{
			AdjustZoom(-ZOOM_IN_OUT_DELTA);
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
			var extensions = MarkPadExtensionsProvider.Extensions.OfType<IDocumentViewExtension>();
			var extensionsToAdd = extensions.Except(this.extensions).ToList();
			var extensionsToRemove = this.extensions.Except(extensions).ToList();

			foreach (var extension in extensionsToAdd)
			{
				extension.ConnectToDocumentView(this);
				this.extensions.Add(extension);
			}

			foreach (var extension in extensionsToRemove)
			{
				extension.DisconnectFromDocumentView(this);
				this.extensions.Remove(extension);
			}
		}


        void WebControl_LinkClicked(object sender, OpenExternalLinkEventArgs e)
        {
            Process.Start(e.Url);
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
            return Constants.FONT_SIZE_ENUM_ADJUSTMENT + ((DocumentViewModel)DataContext).GetFontSize();
        }

		private FontFamily GetFontFamily()
		{
			var documentViewModel = (DocumentViewModel)DataContext;
			return documentViewModel.GetFontFamily();
		}

        /// <summary>
        /// Turn the font size into a zoom level for the browser.
        /// </summary>
        /// <returns></returns>
        private int GetZoomLevel(double fontSize)
        {
            // The default font size 12 corresponds to 100 (which maps to 0 here); for an increment of 1, we add 50/6 to the number.
            // For 18 we end up with 150, which looks really fine. TODO: Feel free to try to further outline this, but this is a good start.
            var zoom = 100.0 + (fontSize - (double)Constants.FONT_SIZE_ENUM_ADJUSTMENT) * 40.0 / 6.0;

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

            if (documentScrollViewer != null)
            {
                documentScrollViewer.ScrollChanged += (i, j) => WbProcentualZoom();
                var x = ((DocumentViewModel)DataContext);
                x.Document.TextChanged += (i, j) =>
                    {
                        wb.LoadCompleted += (k, l) => WbProcentualZoom();
                    };
            }

            // AvalonEdit hijacks Ctrl+I. We need to free that mutha up
            var editCommandBindings = Editor.TextArea.DefaultInputHandler.Editing.CommandBindings;

            editCommandBindings
                .FirstOrDefault(b => b.Command == ICSharpCode.AvalonEdit.AvalonEditCommands.IndentSelection)
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
			var settings = settingsProvider.GetSettings<MarkpadSettings>();
			
			if (!settings.FloatingToolBarEnabled)
				return;
				
            if (Editor.TextArea.Selection.IsEmpty)
                floatingToolBar.Hide();
            else
                floatingToolBar.Show();
        }

		void HandleEditorMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) return;
			ZoomSlider.Value += (double)e.Delta * 0.1;
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

                            sb.AppendLine(string.Format("![{0}]({1})", Path.GetFileNameWithoutExtension(relativePath), relativePath));
                        }

                        textArea.Selection.ReplaceSelectionWithText(textArea, sb.ToString().Trim());
                        e.Handled = true;
                    }
                });
        }
    }
}