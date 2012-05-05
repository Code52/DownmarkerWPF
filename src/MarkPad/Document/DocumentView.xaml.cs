using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit;
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
        private ScrollViewer documentScrollViewer;
        private readonly IList<IDocumentViewExtension> extensions = new List<IDocumentViewExtension>();
        private readonly ISettingsProvider settingsProvider;

        MarkPadSettings settings;

        #region public double ScrollPercentage
        public static DependencyProperty ScrollPercentageProperty = DependencyProperty.Register("ScrollPercentage", typeof(double), typeof(DocumentView),
            new PropertyMetadata(default(double)));

        public double ScrollPercentage
        {
            get { return (double)GetValue(ScrollPercentageProperty); }
            set { SetValue(ScrollPercentageProperty, value); }
        }
        #endregion

        public DocumentView(ISettingsProvider settingsProvider)
        {
            this.settingsProvider = settingsProvider;

            InitializeComponent();

            Loaded += DocumentViewLoaded;
            SizeChanged += DocumentViewSizeChanged;
            markdownEditor.Editor.MouseWheel += HandleEditorMouseWheel;
            markdownEditor.Editor.KeyDown += WordCount_KeyDown;

            Handle(new SettingsChangedEvent());

            CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomIn, (x, y) => ViewModel.ExecuteSafely(vm=>vm.ZoomIn())));
            CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomOut, (x, y) => ViewModel.ExecuteSafely(vm=>vm.ZoomOut())));
            CommandBindings.Add(new CommandBinding(DisplayCommands.ZoomReset, (x, y) => ViewModel.ExecuteSafely(vm=>vm.ZoomReset())));
        }

        private DocumentViewModel ViewModel
        {
            get { return DataContext as DocumentViewModel; }
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
            ApplyExtensions();
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

        public void Cleanup()
        {
            if (htmlPreview.wb != null)
                htmlPreview.wb.Close();
        }

        public void Print()
        {
            htmlPreview.wb.Print();
        }
    }
}