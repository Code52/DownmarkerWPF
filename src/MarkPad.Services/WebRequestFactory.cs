using System;
using System.Net;
using MarkPad.Services.Metaweblog.Rsd;

namespace MarkPad.Services
{
    public class WebRequestFactory : IWebRequestFactory
    {
        public WebRequest Create(Uri requestUri)
        {
            return WebRequest.Create(requestUri);
        }
    }
}