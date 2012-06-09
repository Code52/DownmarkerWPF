using System.Windows;
using MarkPad.Services;

namespace MarkPad.PreviewControl
{
    public partial class HtmlPreview
    {
        #region public string Html
        public static DependencyProperty HtmlProperty = DependencyProperty.Register("Html", typeof (string), typeof (HtmlPreview),
            new PropertyMetadata(" ", HtmlChanged));

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }
        #endregion

        #region public string Filename
        public static readonly DependencyProperty FilenameProperty =
            DependencyProperty.Register("Filename", typeof (string), typeof (HtmlPreview), new PropertyMetadata(default(string)));

        public string Filename
        {
            get { return (string)GetValue(FilenameProperty); }
            set { SetValue(FilenameProperty, value); }
        }
        #endregion

        #region public double BrowserFontSize
        public static DependencyProperty BrowserFontSizeProperty = DependencyProperty.Register("BrowserFontSize", typeof(double), typeof(HtmlPreview),
            new PropertyMetadata(default(double), FontSizeChanged));

        public double BrowserFontSize
        {
            get { return (double)GetValue(BrowserFontSizeProperty); }
            set { SetValue(BrowserFontSizeProperty, value); }
        }
        #endregion

        private AwesomiumHost host;

        #region public int ScrollPercentage
        public static DependencyProperty ScrollPercentageProperty = DependencyProperty.Register("ScrollPercentage", typeof(double), typeof(HtmlPreview),
            new PropertyMetadata(default(double), ScrollPercentageChanged));


        public double ScrollPercentage 
        {
            get { return (double)GetValue(ScrollPercentageProperty); }
            set { SetValue(ScrollPercentageProperty, value); }
        }
        #endregion


        public HtmlPreview()
        {
            InitializeComponent();
        }

        private static void ScrollPercentageChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var awesomiumHost = ((HtmlPreview) dependencyObject).host;
            awesomiumHost.ScrollPercentage = (int)dependencyPropertyChangedEventArgs.NewValue;
            awesomiumHost.WbProcentualZoom();
        }

        private static void FontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HtmlPreview)d).host.SetZoom(GetZoomLevel((double)e.NewValue));
        }

        private static void HtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var content = e.NewValue as string;
            if (string.IsNullOrEmpty(content))
                content = " ";

            var htmlPreview = (HtmlPreview) d;
            htmlPreview.host.SetHtml(content);
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

        public void Close()
        {
            host.Dispose();
        }

        public void Print()
        {
            host.Print();
        }

        private void HtmlPreviewLoaded(object sender, RoutedEventArgs e)
        {
            host = new AwesomiumHost(Filename);

            Content = host.Host;
        }
    }
}
