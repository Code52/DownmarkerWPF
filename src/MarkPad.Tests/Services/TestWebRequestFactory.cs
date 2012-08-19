using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MarkPad.Infrastructure.Abstractions;
using NSubstitute;

namespace MarkPad.Tests.Services
{
    public class TestWebRequestFactory : IWebRequestFactory
    {
        private readonly Dictionary<string, string> lookup = new Dictionary<string, string>();

        public void RegisterResultForUri(string uri, string resultBody)
        {
            lookup.Add(uri.TrimEnd('/'), resultBody);
        }

        public WebRequest Create(Uri requestUri)
        {
            var uri = requestUri.ToString().TrimEnd('/');
            if (lookup.ContainsKey(uri))
                return CreateWebRequest(lookup[uri]);

            Trace.WriteLine(string.Format("No result setup for URI {0}", uri));

            return null;
        }

        private static WebRequest CreateWebRequest(string toReturn)
        {
            var request = Substitute.For<WebRequest>();

            request
                .BeginGetResponse(Arg.Any<AsyncCallback>(), Arg.Any<object>())
                .Returns(c=>
                {
                    var fromResult = TaskEx.FromResult(toReturn);
                    c.Arg<AsyncCallback>()(fromResult);
                    return fromResult;
                });

            request
                .EndGetResponse(Arg.Any<IAsyncResult>())
                .Returns(c=>
                {
                    var response = Substitute.For<WebResponse>();
                    var byteArray = Encoding.ASCII.GetBytes(toReturn);
                    var stream = new MemoryStream(byteArray);
                    response
                        .GetResponseStream()
                        .Returns(stream);
                    return response;
                });

            return request;
        }
    }
}