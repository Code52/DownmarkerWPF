using System;
using System.IO;
using System.Linq;
using CefSharp;

namespace MarkPad.Preview
{
    public class ThemeSchemeHandlerFactory : ISchemeHandlerFactory
    {
        public ISchemeHandler Create()
        {
            return new ThemeSchemeHandler();
        }
    }

    public class ThemeSchemeHandler : ISchemeHandler
    {
        public bool ProcessRequest(IRequest request, ref string mimeType, ref System.IO.Stream stream)
        {
            var uri = new Uri(request.Url);
            var segments = uri.Segments;

            var path = Path.Combine(HtmlPreview.BaseDirectory,
                string.Concat(uri.Segments.Skip(1).Select(p => p.Replace("/", "\\"))));

            var file = new FileInfo(path);

            if (file.Exists)
            {
                stream = file.OpenRead();
                mimeType = "text/html";
                return true;
            }

            return false;
        }
    }
}