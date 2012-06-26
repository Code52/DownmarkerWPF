using System;
using System.Net;

namespace MarkPad.Infrastructure.Abstractions
{
    public interface IWebRequestFactory
    {
        WebRequest Create(Uri requestUri);
    }
}