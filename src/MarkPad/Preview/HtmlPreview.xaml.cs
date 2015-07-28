using CefSharp;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MarkPad.Preview
{
    public partial class HtmlPreview : UserControl
    {
        public static void Init()
        {
            CefSharp.CefSettings settings = new CefSharp.CefSettings();
            settings.RegisterScheme(new CefCustomScheme
            {
                SchemeName = "theme",
                SchemeHandlerFactory = new ThemeSchemeHandlerFactory()
            });
            if (Cef.Initialize(settings))
            {
                //CEF.RegisterScheme("test", new SchemeHandlerFactory());
                //CEF.RegisterJsObject("bound", new BoundObject());
            }
        }

        public static string BaseDirectory { get; set; }

        #region public string Html

        public static DependencyProperty HtmlProperty =
            DependencyProperty.Register("Html", typeof(string), typeof(HtmlPreview), new PropertyMetadata(" ", HtmlChanged));

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }

        #endregion public string Html

        #region public string FileName

        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(HtmlPreview), new PropertyMetadata("", FileNameChanged));

        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        #endregion public string FileName

        #region public double ScrollPercentage

        public static DependencyProperty ScrollPercentageProperty =
            DependencyProperty.Register("ScrollPercentage", typeof(double), typeof(HtmlPreview), new PropertyMetadata(0d, ScrollPercentageChanged));

        public string ScrollPercentage
        {
            get { return (string)GetValue(ScrollPercentageProperty); }
            set { SetValue(ScrollPercentageProperty, value); }
        }

        public double LastScrollPercentage;

        #endregion public double ScrollPercentage

        #region FontSize

        private static void FonSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var htmlPreview = (HtmlPreview)d;
            if (htmlPreview.host != null && htmlPreview.host.IsBrowserInitialized)
                htmlPreview.UpdateZoomLevel((double)e.NewValue);
        }

        #endregion

        static HtmlPreview()
        {
            FontSizeProperty.OverrideMetadata(typeof(HtmlPreview), new FrameworkPropertyMetadata(FonSizeChanged));
        }

        public HtmlPreview()
        {
            InitializeComponent();
        }

        public void Print()
        {
            if (host.IsBrowserInitialized)
                host.Print();
        }

        public void RestoreLastScrollPercentage()
        {
            ExecuteScroll(this, LastScrollPercentage);
        }

        private static void HtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var htmlPreview = (HtmlPreview)d;
            if (htmlPreview.host != null && htmlPreview.host.IsBrowserInitialized)
            {
                var newValue = (string)e.NewValue;
                if (newValue == null)
                    htmlPreview.host.LoadHtml(string.Empty, string.Empty);
                else
                {
                    htmlPreview.host.LoadHtml(newValue, string.Format("http://{0}/", htmlPreview.FileName));
                    htmlPreview.RestoreLastScrollPercentage();
                }
            }
        }

        private static void FileNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var htmlPreview = (HtmlPreview)d;
            if (htmlPreview.host != null && htmlPreview.host.IsBrowserInitialized)
                htmlPreview.host.Title = (string)e.NewValue;
        }

        private static void ExecuteScroll(HtmlPreview htmlPreview, object scrollPercentage)
        {
            var javascript = string.Format("window.scrollTo(0,{0} * (document.body.scrollHeight - document.body.clientHeight));", scrollPercentage);
            htmlPreview.host.ExecuteScriptAsync(javascript);
        }

        private static void ScrollPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var htmlPreview = (HtmlPreview)d;
            if (htmlPreview.host != null && htmlPreview.host.IsBrowserInitialized)
            {
                ExecuteScroll(htmlPreview, e.NewValue);
                htmlPreview.LastScrollPercentage = (double)e.NewValue;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            host.IsBrowserInitializedChanged += Host_IsBrowserInitializedChanged;
        }

        private void Host_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (host.IsBrowserInitialized)
                Dispatcher.BeginInvoke((Action)InitializeData);
        }

        private void InitializeData()
        {
            host.LoadHtml(Html, string.Format("http://{0}/", FileName));
            host.Title = FileName;
        }

        private async void host_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {
            var fontSize = await Dispatcher.InvokeAsync(() => FontSize);
            UpdateZoomLevel(fontSize);
        }

        public async void UpdateZoomLevel(double fontSize)
        {
            double scale = await Dispatcher.InvokeAsync(() =>
            {
                // TODO: Can be optimized
                PresentationSource source = PresentationSource.FromVisual(this);
                return source.CompositionTarget.TransformToDevice.M11;
            });
            host.SetZoomLevel(fontSize * scale * 2 / Constants.FONT_SIZE_ENUM_ADJUSTMENT);
        }
    }
}