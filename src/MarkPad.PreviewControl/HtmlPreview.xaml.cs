using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MarkPad.PreviewControl
{
    public partial class HtmlPreview
    {
        static string content;
        AwesomiumHost host;
        AppDomain hostAppDomain;
        HwndContentHost hwndContentHost;

        public static string BaseDirectory { get; set; }

        #region public string Html
        public static DependencyProperty HtmlProperty = DependencyProperty.Register("Html", typeof (string), typeof (HtmlPreview),
            new PropertyMetadata(" ", HtmlChanged));

        public string Html
        {
            get { return (string)GetValue(HtmlProperty); }
            set { SetValue(HtmlProperty, value); }
        }
        #endregion

        #region public string FileName
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof (string), typeof (HtmlPreview), new PropertyMetadata(default(string)));

        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
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
            awesomiumHost.ScrollPercentage = (double)dependencyPropertyChangedEventArgs.NewValue;
            awesomiumHost.WbProcentualZoom();
        }

        private static void FontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var htmlPreview = ((HtmlPreview) d);
            if (htmlPreview.host != null)
                htmlPreview.host.SetZoom(GetZoomLevel((double)e.NewValue));
        }

        private static void HtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            content = e.NewValue as string;
            if (string.IsNullOrEmpty(content))
                content = " ";

            var htmlPreview = (HtmlPreview) d;
            if (htmlPreview.host != null)
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
            Loaded -= HtmlPreviewLoaded;
            var context = TaskScheduler.FromCurrentSynchronizationContext();

            //This is a perf helper, the MetroContentControl slides content in
            // The preview pane stutters and looks crap, but hiding it then showing it after the animation it looks way better
            Unloaded += (o, args) =>
                            {
                                if (hwndContentHost == null) return;
                                hwndContentHost.Visibility = Visibility.Hidden;
                            };
            Loaded +=
                (o, args) =>
                {
                    var delay = TaskEx.Delay(500);
                    delay.ContinueWith(t =>
                    {
                        hwndContentHost.Visibility = Visibility.Visible;
                    }, context);
                };

            //We are hosting the Awesomium preview in another appdomain so our main UI thread does not take the hit
            hostAppDomain = AppDomain.CreateDomain("HtmlPreviewDomain");
            var filename = FileName;

            // create the AppDomain on a new thread as we want to ensure it is an 
            // STA thread as this makes life easier for creating UI components
            var thread = new Thread(() =>
            {
                var awesomiumHostType = typeof(AwesomiumHost);
                host = (AwesomiumHost)hostAppDomain.CreateInstanceAndUnwrap(awesomiumHostType.Assembly.FullName, awesomiumHostType.FullName,
                false, BindingFlags.Default, null, new object[] { filename, BaseDirectory }, CultureInfo.CurrentCulture, null);

                host.SetHtml(content);

                // We need to invoke on the Markpad dispatcher, we are currently in the host appdomains STA Thread.
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var controlHandle = host.ControlHandle;

                    hwndContentHost = new HwndContentHost(controlHandle);
                    //Without the border we don't get the dropshadows
                    Content = new Border
                    {
                        Background = Brushes.White,
                        Padding = new Thickness(3),
                        Child = hwndContentHost
                    };
                }));            

                host.Run();
                //I can't get this unloading without an error, 
                // I am gathering Application.Shutdown is causing the appdomain to shutdown too
                //AppDomain.Unload(hostAppDomain);
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }
}
