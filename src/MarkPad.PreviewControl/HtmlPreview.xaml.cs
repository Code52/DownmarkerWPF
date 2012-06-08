using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Awesomium.Core;
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

        #region public int ScrollPercentage
        public static DependencyProperty ScrollPercentageProperty = DependencyProperty.Register("ScrollPercentage", typeof(double), typeof(HtmlPreview),
            new PropertyMetadata(default(double), ScrollPercentageChanged));

        public double ScrollPercentage 
        {
            get { return (double)GetValue(ScrollPercentageProperty); }
            set { SetValue(ScrollPercentageProperty, value); }
        }
        #endregion

        const string LocalRequestUrlBase = "local://base_request.html/";

        public HtmlPreview()
        {
            InitializeComponent();
            wb.Loaded += WbLoaded;
            wb.OpenExternalLink += WebControlLinkClicked;
            wb.ResourceRequest += WebControlResourceRequest;
        }

        private static void ScrollPercentageChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((HtmlPreview)dependencyObject).WbProcentualZoom();
        }

        void WbLoaded(object sender, RoutedEventArgs e)
        {
            WbProcentualZoom();
        }

        private static void FontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HtmlPreview)d).wb.Zoom = GetZoomLevel((double)e.NewValue);
        }

        private static void HtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var content = e.NewValue as string;
            if (string.IsNullOrEmpty(content))
                content = " ";

            var htmlPreview = (HtmlPreview) d;
            var webControl = htmlPreview.wb;
            webControl.CacheMode = new BitmapCache();
            EventHandler webControlOnLoadCompleted = null;
            webControlOnLoadCompleted = (sender, args) =>
            {
                webControl.LoadCompleted -= webControlOnLoadCompleted;
                htmlPreview.WbProcentualZoom();
                webControl.CacheMode = null;
            };
            webControl.LoadCompleted += webControlOnLoadCompleted;
            webControl.LoadHTML(content);
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

        public string GetResourceFilename(string url)
        {
            if (string.IsNullOrEmpty(Filename)) return null;

            var resourceFilename = Path.Combine(Path.GetDirectoryName(Filename), url);
            return resourceFilename;
        }

        private void WbProcentualZoom()
        {
            wb.ExecuteJavascript("window.scrollTo(0," + ScrollPercentage + " * (document.body.scrollHeight - document.body.clientHeight));");
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
                var encoding = new UTF8Encoding();

                return new ResourceResponse(encoding.GetBytes(Html), "text/html");
            }

            var resourceFilename = GetResourceFilename(url);
            if (!File.Exists(resourceFilename)) return null;

            return new ResourceResponse(resourceFilename);
        }

        public void Close()
        {
            if (wb != null)
                wb.Close();
        }

        public void Print()
        {
            if (wb != null)
                wb.Print();
        }
    }
}