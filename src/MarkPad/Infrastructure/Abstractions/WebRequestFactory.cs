using System;
using System.Net;

namespace MarkPad.Infrastructure.Abstractions
{
    public class WebRequestFactory : IWebRequestFactory
    {
        public WebRequest Create(Uri requestUri)
        {
            return WebRequest.Create(requestUri);
        }
    }
}