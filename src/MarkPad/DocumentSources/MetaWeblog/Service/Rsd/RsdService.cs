using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using MarkPad.Infrastructure.Abstractions;

namespace MarkPad.DocumentSources.MetaWeblog.Service.Rsd
{
    public class RsdService : IRsdService
    {
        const string RsdNamespace = "http://archipelago.phrasewise.com/rsd";
        readonly IWebRequestFactory webRequestFactory;

        public RsdService(IWebRequestFactory webRequestFactory)
        {
            this.webRequestFactory = webRequestFactory;
        }

        public Task<DiscoveryResult> DiscoverAddress(string webAPI)
        {
            var completionSource = new TaskCompletionSource<DiscoveryResult>();

            var baseUri = new Uri(webAPI, UriKind.Absolute);
            var requestUri = new Uri(baseUri, "rsd.xml");
            var rsdFileRequest = webRequestFactory.Create(requestUri);

            // Kick off the async discovery workflow
            rsdFileRequest.GetResponseAsync()
                .ContinueWith<DiscoveryResult>(ProcessRsdResponse)
                .ContinueWith(c =>
                {
                    if (c.Result.Success)
                        completionSource.SetResult(c.Result);
                    else
                    {
                        Trace.WriteLine(string.Format(
                            "Rsd.xml does not exist, trying to discover via link. Error was {0}", c.Result.FailMessage), "INFO");

                        DiscoverRsdLink(webAPI)
                            .ContinueWith(t => completionSource.SetResult(t.Result));
                    }
                });

            return completionSource.Task;
        }

        Task<DiscoveryResult> DiscoverRsdLink(string webAPI)
        {
            var taskCompletionSource = new TaskCompletionSource<DiscoveryResult>();

            // Build a request to retrieve the contents of the specified URL directly
            var requestUri = new Uri(webAPI, UriKind.Absolute);
            var directWebAPIRequest = webRequestFactory.Create(requestUri);
            
            // Add a continuation that will only execute if the request succeeds and proceses the response to look for a <link> to the RSD
            directWebAPIRequest.GetResponseAsync()
                .ContinueWith(webAPIRequestAntecedent =>
            {
                if (webAPIRequestAntecedent.IsFaulted)
                {
                    taskCompletionSource.SetResult(new DiscoveryResult(webAPIRequestAntecedent.Exception));
                    return;
                }
                using (var webAPIResponse = webAPIRequestAntecedent.Result)
                using (var streamReader = new StreamReader(GetResponseStream(webAPIResponse)))
                {
                    DiscoverRsdOnPage(webAPI, streamReader, taskCompletionSource);
                }
            });

            return taskCompletionSource.Task;
        }

        void DiscoverRsdOnPage(string webAPI, TextReader streamReader, TaskCompletionSource<DiscoveryResult> taskCompletionSource)
        {
            const string linkTagRegex = "(?<link>\\<link .*?type=\"application/rsd\\+xml\".*?/\\>)";

            var response = streamReader.ReadToEnd();
            var link = Regex.Match(response, linkTagRegex, RegexOptions.IgnoreCase);
            var rsdLinkMatch = link.Groups["link"];

            if (!rsdLinkMatch.Success)
            {
                taskCompletionSource.SetResult(DiscoveryResult.Failed("Unable to resolve link to rsd file from url"));
                return;
            }

            var rsdLocationMatch = Regex.Match(rsdLinkMatch.Value, "href=(?:\"|')(?<link>.*?)(?:\"|')");
            if (!rsdLocationMatch.Groups["link"].Success)
            {
                taskCompletionSource.SetResult(DiscoveryResult.Failed("Unable to parse rsd link tag"));
                return;
            }

            var rsdUri = new Uri(rsdLocationMatch.Groups["link"].Value, UriKind.RelativeOrAbsolute);
            if (!rsdUri.IsAbsoluteUri)
                rsdUri = new Uri(new Uri(webAPI, UriKind.Absolute), rsdUri);

            var rdsWebRequest = webRequestFactory.Create(rsdUri);
            var rdsWebRequestTask = rdsWebRequest.GetResponseAsync();

            // Add a continuation that will only execute if the request succeeds and continues processing the RSD
            rdsWebRequestTask.ContinueWith(rdsWebRequestAntecedent =>
                                           taskCompletionSource.SetResult(ProcessRsdResponse(rdsWebRequestAntecedent)),
                                           TaskContinuationOptions.NotOnFaulted);

            // Add a continuation that will only execute if the request faults and propagates the exception via the TCS
            rdsWebRequestTask.ContinueWith(rdsWebRequestAntecdent =>
                                           taskCompletionSource.SetResult(new DiscoveryResult(rdsWebRequestAntecdent.Exception)),
                                           TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);
        }

        private static Stream GetResponseStream(WebResponse webAPIResponse)
        {
            return webAPIResponse.GetResponseStream();
        }

        static DiscoveryResult ProcessRsdResponse(Task<WebResponse> webResponseTask)
        {
            if (webResponseTask.IsFaulted)
                return new DiscoveryResult(webResponseTask.Exception);

            try
            {
                using (var webResponse = webResponseTask.Result)
                {
                    using (var responseStream = webResponse.GetResponseStream())
                    {
                        var document = XDocument.Load(responseStream);
                        var apiElement = GetMetaWebLogElement(document);
                        if (apiElement == null)
                            return DiscoveryResult.Failed("Unable to get metaweblog api address from rds.xml");

                        var xAttribute = apiElement.Attribute("apiLink");
                        if (xAttribute == null)
                            return DiscoveryResult.Failed("apiLink attribute not present for metaweblog api reference");

                        var webApiLink = xAttribute.Value;
                        return new DiscoveryResult(webApiLink);
                    }
                }
            }
            catch (Exception ex)
            {
                return new DiscoveryResult(ex);
            }
        }

        private static XElement GetMetaWebLogElement(XDocument document)
        {
            // ReSharper disable PossibleNullReferenceException
            try
            {
                IEnumerable<XElement> apiElements;
                if (document.Root.Attributes().Any(x => x.IsNamespaceDeclaration && x.Value == RsdNamespace))
                {
                    apiElements = document
                        .Element(XName.Get("rsd", RsdNamespace))
                        .Element(XName.Get("service", RsdNamespace))
                        .Element(XName.Get("apis", RsdNamespace))
                        .Elements(XName.Get("api", RsdNamespace));
                }
                else
                {
                    apiElements = document
                        .Element(XName.Get("rsd"))
                        .Element(XName.Get("service"))
                        .Element(XName.Get("apis"))
                        .Elements(XName.Get("api"));
                }

                return apiElements.SingleOrDefault(e => e.Attribute("name").Value.ToLower() == "metaweblog");
            }
            catch (NullReferenceException)
            {
                return null;
            }
            // ReSharper restore PossibleNullReferenceException
        }
    }
}