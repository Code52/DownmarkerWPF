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

        public HtmlPreview()
        {
            InitializeComponent();
        }

        public void Print()
        {
            if (host.IsBrowserInitialized)
                host.Print();
        }

        private static void HtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var htmlPreview = (HtmlPreview)d;
            if (htmlPreview.host != null && htmlPreview.host.IsBrowserInitialized)
                htmlPreview.host.LoadHtml((string)e.NewValue);
        }

        private static void FileNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var htmlPreview = (HtmlPreview)d;
            if (htmlPreview.host != null && htmlPreview.host.IsBrowserInitialized)
                htmlPreview.host.Title = (string)e.NewValue;
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