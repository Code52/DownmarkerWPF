using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Awesomium.Core;
using Awesomium.Windows.Controls;

namespace MarkPad.PreviewControl
{
    public class AwesomiumHost : IDisposable
    {
        readonly string filename;
        readonly WebControl wb;
        string html;
        const string LocalRequestUrlBase = "local://base_request.html/";

        public AwesomiumHost(string filename)
        {
            this.filename = filename;

            wb = new WebControl
            {
                UseLayoutRounding = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Effect = new DropShadowEffect
                {
                    BlurRadius = 10,
                    Color = Colors.Black,
                    Opacity = 0.25,
                    Direction = 270
                }
            };
            wb.Loaded += WbLoaded;
            wb.OpenExternalLink += WebControlLinkClicked;
            wb.ResourceRequest += WebControlResourceRequest;
        }

        void WbLoaded(object sender, RoutedEventArgs e)
        {
            WbProcentualZoom();
        }

        public int ScrollPercentage { get; set; }

        public int ControlHandle
        {
            get { return 0; }
        }

        public object Host { get { return wb; } }

        public void SetZoom(int getZoomLevel)
        {
            wb.Zoom = getZoomLevel;
        }

        public void SetHtml(string content)
        {
            html = content;
            var webControl = wb;
            webControl.CacheMode = new BitmapCache();
            EventHandler webControlOnLoadCompleted = null;
            webControlOnLoadCompleted = (sender, args) =>
            {
                webControl.LoadCompleted -= webControlOnLoadCompleted;
                WbProcentualZoom();
                webControl.CacheMode = null;
            };
            webControl.LoadCompleted += webControlOnLoadCompleted;
            webControl.LoadHTML(content);
        }

        public void WbProcentualZoom()
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

                return new ResourceResponse(encoding.GetBytes(html), "text/html");
            }

            var resourceFilename = GetResourceFilename(url);
            if (!File.Exists(resourceFilename)) return null;

            return new ResourceResponse(resourceFilename);
        }

        void WebControlLinkClicked(object sender, OpenExternalLinkEventArgs e)
        {
            // Although all links have "target='_blank'" added (see ParsedDocument.ToHtml()), they go through this first
            // unless the url is local (a bug in Awesomium) in which case this event isn't triggered, and the "target='_blank'"
            // takes over to avoid crashing the preview. Local resource requests where the resource doesn't exist are thrown
            // away. See WebControl_ResourceRequest().

            var file = e.Url;
            if (e.Url.StartsWith(LocalRequestUrlBase))
            {
                file = GetResourceFilename(e.Url.Replace(LocalRequestUrlBase, "")) ?? "";
                if (!File.Exists(file)) return;
            }

            if (string.IsNullOrWhiteSpace(file)) return;

            Process.Start(file);
        }

        public string GetResourceFilename(string url)
        {
            if (string.IsNullOrEmpty(filename)) return null;

            var directoryName = Path.GetDirectoryName(filename);
            if (directoryName == null)
                return null;
            var resourceFilename = Path.Combine(directoryName, url);
            return resourceFilename;
        }

        public void Dispose()
        {
            wb.Close();
        }

        public void Print()
        {
            wb.Print();
        }
    }
}