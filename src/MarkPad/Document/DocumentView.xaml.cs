using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Awesomium.Core;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit;
using MarkPad.Framework;
using MarkPad.Framework.Events;
using MarkPad.MarkPadExtensions;
using MarkPad.Services;
using MarkPad.Services.Interfaces;
using MarkPad.Services.Settings;
using MarkPad.XAML;
using MarkPad.Contracts;
using System.ComponentModel.Composition;
using MarkPad.PluginApi;

namespace MarkPad.Document
{
    public partial class DocumentView : 
		IDocumentView,
		IHandle<SettingsChangedEvent>,
		IHandle<PluginsChangedEvent>
    {
        private const double ZoomDelta = 0.1;
        private const string LocalRequestUrlBase = "local://base_request.html/";

        private ScrollViewer documentScrollViewer;
        private readonly ISettingsProvider settingsProvider;
		private readonly IPluginManager pluginManager;

        MarkPadSettings settings;
		[ImportMany]
		IEnumerable<IDocumentViewPlugin> documentViewPlugins;
		IEnumerable<IDocumentViewPlugin> connectedDocumentViewPlugins = new IDocumentViewPlugin[0];

        public DocumentView(
			ISettingsProvider settingsProvider,
			IPluginManager pluginManager)
        {
			this.settingsProvider = settingsProvider;
			this.pluginManager = pluginManager;

			this.pluginManager.Container.ComposeParts(this);
			
            InitializeComponent();

			UpdatePlugins();

            Loaded += DocumentViewLoaded;
            wb.Loaded += WbLoaded;
            wb.OpenExternalLink += WebControlLinkClicked;
            wb.ResourceRequest += WebControlResourceRequest;
            SizeChanged += DocumentViewSizeChanged;
            ZoomSlider.ValueChanged += (sender, e) => ApplyZoom();
            markdownEditor.Editor.MouseWheel += HandleEditorMouseWheel;
            markdownEditor.Editor.KeyDown += WordCount_KeyDown;

			Handle(new SettingsChangedEvent());

            CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomIn, (x, y) => ZoomIn()));
            CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomOut, (x, y) => ZoomOut()));
            CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomReset, (x, y) => ZoomReset()));
        }

		public void UpdatePlugins()
		{
			var enabledPlugins = documentViewPlugins.Where(p => p.Settings.IsEnabled);

			foreach (var plugin in connectedDocumentViewPlugins.Except(enabledPlugins))
			{
				plugin.DisconnectFromDocumentView(this);
			}
			foreach (var plugin in enabledPlugins.Except(connectedDocumentViewPlugins))
			{
				plugin.ConnectToDocumentView(this);
			}

			connectedDocumentViewPlugins = new List<IDocumentViewPlugin>(enabledPlugins);
		}

        public TextEditor Editor
        {
            get { return markdownEditor.Editor; }
        }

        void HandleEditorMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) return;
            ZoomSlider.Value += e.Delta * 0.1;
        }

        void WordCount_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as DocumentViewModel;

            var count = 0;

            if (!string.IsNullOrEmpty(vm.Render))
            {
                count = GetWordCount(vm.Render);
            }

            WordCount.Content = "words: " + count;
        }

        private static int GetWordCount(string text)
        {
            var input = text;
            input = Regex.Replace(input, @"(?s)<script.*?(/>|</script>)", string.Empty);
            input = Regex.Replace(input, @"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", string.Empty);
            return Regex.Matches(input, @"[\S]+").Count;
        }

        private void ApplyZoom()
        {
            markdownEditor.Editor.TextArea.TextView.Redraw();

            var zoom = ZoomSlider.Value;

            var fontSize = GetFontSize() * zoom;

            markdownEditor.Editor.FontSize = fontSize;
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
            markdownEditor.Editor.FontFamily = GetFontFamily();
        }

        void WebControlLinkClicked(object sender, OpenExternalLinkEventArgs e)
        {
            // Although all links have "target='_blank'" added (see ParsedDocument.ToHtml()), they go through this first
            // unless the url is local (a bug in Awesomium) in which case this event isn't triggered, and the "target='_blank'"
            // takes over to avoid crashing the preview. Local resource requests where the resource doesn't exist are thrown
            // away. See WebControl_ResourceRequest().

            string filename = e.Url;
            if (e.Url.StartsWith(LocalRequestUrlBase))
            {
                filename = GetResourceFilename(e.Url.Replace(LocalRequestUrlBase, "")) ?? "";
                if (!File.Exists(filename)) return;
            }

            if (string.IsNullOrWhiteSpace(filename)) return;

            Process.Start(filename);
        }

        ResourceResponse WebControlResourceRequest(object o, ResourceRequestEventArgs e)
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

            if (e.Request.Url.StartsWith(LocalRequestUrlBase)) return GetLocalResource(e.Request.Url.Replace(LocalRequestUrlBase, ""));

            // If the request wasn't local, return null to let the usual handler load the url from the network			
            return null;
        }
        ResourceResponse GetLocalResource(string url)
        {
			if (string.IsNullOrWhiteSpace(url))
			{
				string result = null;
				var encoding = new System.Text.UTF8Encoding();

				(DataContext as DocumentViewModel).ExecuteSafely(vm => result = vm.Render);

				return new ResourceResponse(encoding.GetBytes(result), "text/html");
			}

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
            documentScrollViewer = markdownEditor.FindVisualChild<ScrollViewer>();

            var viewModel = ((DocumentViewModel)DataContext);
            if (documentScrollViewer != null)
            {
                documentScrollViewer.ScrollChanged += (i, j) => WbProcentualZoom();
                viewModel.Document.TextChanged += (i, j) =>
                {
                    wb.LoadCompleted += (k, l) => WbProcentualZoom();
                };
            }
        }

        public void Handle(SettingsChangedEvent message)
        {
            settings = settingsProvider.GetSettings<MarkPadSettings>();
            markdownEditor.FloatingToolbarEnabled = settings.FloatingToolBarEnabled;

            ApplyFont();
            ApplyZoom();
        }

		public void Handle(PluginsChangedEvent e)
		{
			UpdatePlugins();
		}

        private void SiteFilesMouseDoubleClick(object sender, MouseButtonEventArgs e)
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