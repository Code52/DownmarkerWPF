using System;
using System.Windows;
using System.Windows.Controls;
using CefSharp;

namespace MarkPad.Preview
{
    public partial class HtmlPreview : UserControl
    {
        public static void Init()
        {
            CefSharp.Settings settings = new CefSharp.Settings();
            if (CEF.Initialize(settings))
            {
                CEF.RegisterScheme("theme", new ThemeSchemeHandlerFactory());
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
                htmlPreview.host.LoadHtml((string)e.NewValue);
                htmlPreview.RestoreLastScrollPercentage();
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
            htmlPreview.host.ExecuteScript(javascript);
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

            host.PropertyChanged += host_PropertyChanged;
        }

        private void host_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsBrowserInitialized":
                    if (host.IsBrowserInitialized)
                        Dispatcher.BeginInvoke((Action)InitializeData);
                    break;
            }
        }

        private void InitializeData()
        {
            host.LoadHtml(Html);
            host.Title = FileName;
        }
    }
}