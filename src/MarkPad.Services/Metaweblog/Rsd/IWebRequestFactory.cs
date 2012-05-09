using System;
using System.Net;

namespace MarkPad.Services.Metaweblog.Rsd
{
    public interface IWebRequestFactory
    {
        WebRequest Create(Uri requestUri);
    }
}