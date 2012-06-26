using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using MarkPad.Document.Commands;
using MarkPad.Events;
using MarkPad.Framework;
using MarkPad.Helpers;
using MarkPad.Infrastructure.Plugins;
using MarkPad.Plugins;
using MarkPad.Settings;
using MarkPad.Settings.Models;
using MarkPad.Contracts;
using System.ComponentModel.Composition;

namespace MarkPad.Document
{
    public partial class DocumentView : 
		IDocumentView,
		IHandle<SettingsChangedEvent>,
		IHandle<PluginsChangedEvent>
    {
        MarkPadSettings settings;
        ScrollViewer documentScrollViewer;
        readonly ISettingsProvider settingsProvider;
		readonly IPluginManager pluginManager;

		[ImportMany]
		IEnumerable<IDocumentViewPlugin> documentViewPlugins;
		IEnumerable<IDocumentViewPlugin> connectedDocumentViewPlugins = new IDocumentViewPlugin[0];

        #region public double ScrollPercentage
        public static DependencyProperty ScrollPercentageProperty = DependencyProperty.Register("ScrollPercentage", typeof(double), typeof(DocumentView),
            new PropertyMetadata(default(double)));

        public double ScrollPercentage
        {
            get { return (double)GetValue(ScrollPercentageProperty); }
            set { SetValue(ScrollPercentageProperty, value); }
        }
        #endregion

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
            SizeChanged += DocumentViewSizeChanged;
            markdownEditor.Editor.MouseWheel += HandleEditorMouseWheel;

			Handle(new SettingsChangedEvent());

            CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomIn, (x, y) => ViewModel.ExecuteSafely(vm=>vm.ZoomIn())));
            CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomOut, (x, y) => ViewModel.ExecuteSafely(vm=>vm.ZoomOut())));
            CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomReset, (x, y) => ViewModel.ExecuteSafely(vm=>vm.ZoomReset())));
        }

        public TextView TextView
        {
            get { return Editor.TextArea.TextView; }
        }

        public TextDocument Document
        {
            get { return Editor.Document; }
        }

        private DocumentViewModel ViewModel
        {
            get { return DataContext as DocumentViewModel; }
        }

		public void UpdatePlugins()
		{
			var enabledPlugins = documentViewPlugins.Where(p => p.Settings.IsEnabled).ToArray();

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
            ViewModel.ExecuteSafely(vm => vm.ZoomLevel += e.Delta*0.1);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) return;

            e.Handled = true;

            if (e.Delta > 0)
                ViewModel.ExecuteSafely(vm=>vm.ZoomIn());
            else 
                ViewModel.ExecuteSafely(vm=>vm.ZoomOut());
        }

        private void ApplyFont()
        {
            markdownEditor.Editor.FontFamily = GetFontFamily();
        }

        void DocumentViewSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Hide web browser when the window is too small for it to make much sense
            webBrowserColumn.MaxWidth = e.NewSize.Width <= 350 ? 0 : double.MaxValue;
        }

        private FontFamily GetFontFamily()
        {
            var configuredSource = settings.FontFamily;
            var fontFamily = FontHelpers.TryGetFontFamilyFromStack(configuredSource, "Segoe UI", "Arial");
            if (fontFamily == null) throw new Exception("Cannot find configured font family or fallback fonts");
            return fontFamily;
        }

        private void DocumentViewLoaded(object sender, RoutedEventArgs e)
        {
            documentScrollViewer = markdownEditor.FindVisualChild<ScrollViewer>();

            if (documentScrollViewer != null)
            {
                documentScrollViewer.ScrollChanged += DocumentScrollViewerOnScrollChanged;
            }
        }

        private void DocumentScrollViewerOnScrollChanged(object sender, ScrollChangedEventArgs scrollChangedEventArgs)
        {
            ScrollPercentage = documentScrollViewer.VerticalOffset / (documentScrollViewer.ExtentHeight - documentScrollViewer.ViewportHeight);
        }

        public void Handle(SettingsChangedEvent message)
        {
            settings = settingsProvider.GetSettings<MarkPadSettings>();
            markdownEditor.FloatingToolbarEnabled = settings.FloatingToolBarEnabled;

            //TODO this whole settings handler needs to be moved into viewmodel.
            ApplyFont();
            markdownEditor.Editor.TextArea.TextView.Redraw();
            ViewModel.ExecuteSafely(vm => vm.RefreshFont());
        }

		public void Handle(PluginsChangedEvent e)
		{
			UpdatePlugins();
		}

        void PlaceHolderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            PreviewWidth = previewPlaceHolder.ActualWidth;
            PreviewHeight = previewPlaceHolder.ActualHeight;
        }

        public static readonly DependencyProperty PreviewWidthProperty =
            DependencyProperty.Register("PreviewWidth", typeof (double), typeof (DocumentView), new PropertyMetadata(default(double)));

        public double PreviewWidth
        {
            get { return (double) GetValue(PreviewWidthProperty); }
            set { SetValue(PreviewWidthProperty, value); }
        }

        public static readonly DependencyProperty PreviewHeightProperty =
            DependencyProperty.Register("PreviewHeight", typeof (double), typeof (DocumentView), new PropertyMetadata(default(double)));

        public double PreviewHeight
        {
            get { return (double) GetValue(PreviewHeightProperty); }
            set { SetValue(PreviewHeightProperty, value); }
        }
    }
}