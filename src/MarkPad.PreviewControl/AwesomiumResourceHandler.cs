using System.Diagnostics;
using System.IO;
using System.Text;
using Awesomium.Core;

namespace MarkPad.PreviewControl
{
    class AwesomiumResourceHandler
    {
        const string LocalRequestUrlBase = "local://base_request.html/";
        public static AwesomiumHost Host { get; set; }

        public static ResourceResponse WebControlResourceRequest(object o, ResourceRequestEventArgs e)
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

        public static void WebControlLinkClicked(object sender, OpenExternalLinkEventArgs e)
        {
            // Although all links have "target='_blank'" added (see ParsedDocument.ToHtml()), they go through this first
            // unless the url is local (a bug in Awesomium) in which case this event isn't triggered, and the "target='_blank'"
            // takes over to avoid crashing the preview. Local resource requests where the resource doesn't exist are thrown
            // away. See WebControl_ResourceRequest().

            var file = e.Url;
            if (e.Url.StartsWith(LocalRequestUrlBase))
            {
                file = GetResourceFileName(e.Url.Replace(LocalRequestUrlBase, "")) ?? "";
                if (!File.Exists(file)) return;
            }

            if (string.IsNullOrWhiteSpace(file)) return;

            Process.Start(file);
        }

        public static string GetResourceFileName(string url)
        {
            if (string.IsNullOrEmpty(Host.FileName)) return null;

            var directoryName = Path.GetDirectoryName(Host.FileName);
            if (directoryName == null)
                return null;
            var resourceFileName = Path.Combine(directoryName, url);
            return resourceFileName;
        }

        static ResourceResponse GetLocalResource(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                var encoding = new UTF8Encoding();

                return new ResourceResponse(encoding.GetBytes(Host.Html), "text/html");
            }

            var resourceFileName = GetResourceFileName(url);
            if (!File.Exists(resourceFileName)) return null;

            return new ResourceResponse(resourceFileName);
        }
    }
}