using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Caliburn.Micro;
using MarkPad.Metaweblog;
using MarkPad.Services.Interfaces;

namespace MarkPad.Settings
{
    public class BlogSettingsViewModel : Screen
    {
        private readonly IDialogService dialogService;

        public BlogSettingsViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            BlogLanguages = new List<string> { "HTML", "Markdown" };
        }

        public override string DisplayName
        {
            get { return "Blog Settings"; }
            set { }
        }

        public void InitializeBlog(BlogSetting blog)
        {
            CurrentBlog = blog;
        }

        public List<string> BlogLanguages { get; set; }

        public string SelectedBlogLanguage
        {
            get
            {
                if (CurrentBlog == null)
                    return "";
                return CurrentBlog.Language ?? "HTML";
            }
            set { CurrentBlog.Language = value; }
        }

        public BlogSetting CurrentBlog { get; set; }

        public ObservableCollection<FetchedBlogInfo> APIBlogs { get; set; }

        public FetchedBlogInfo SelectedAPIBlog
        {
            get
            {
                if (CurrentBlog == null)
                    return null;

                var bi = new FetchedBlogInfo
                         {
                             BlogInfo = CurrentBlog.BlogInfo,
                             Name = CurrentBlog.BlogInfo.blogName
                         };

                if (APIBlogs == null) APIBlogs = new ObservableCollection<FetchedBlogInfo>();

                var listEntry = APIBlogs.SingleOrDefault(b => b.Name == bi.Name);

                if (listEntry == null)
                {
                    APIBlogs.Add(bi);
                    return bi;
                }

                return listEntry;
            }
            set
            {
                if (CurrentBlog == null)
                    return;
                CurrentBlog.BlogInfo = value == null ? new BlogInfo() : value.BlogInfo;
            }
        }

        public void SetCurrentBlogPassword(object password)
        {
            if (CurrentBlog == null)
                return;

            CurrentBlog.Password = password.ToString();
        }

        public void FetchBlogs()
        {
            SelectedAPIBlog = null;

            var proxy = new MetaWeblog(CurrentBlog.WebAPI);

            APIBlogs = new ObservableCollection<FetchedBlogInfo>();

            var taskBlogInfo = Task<BlogInfo[]>.Factory.FromAsync(
                                   proxy.BeginGetUsersBlogs,
                                   proxy.EndGetUsersBlogs,
                                   "MarkPad",
                                   CurrentBlog.Username,
                                   CurrentBlog.Password,
                                   null);

            taskBlogInfo.ContinueWith(continueParam =>
            {
                if (continueParam.Exception != null)
                {
                    var message = continueParam.Exception.Message;

                    var aggEx = continueParam.Exception as AggregateException;
                    if (aggEx != null)
                        message = String.Join(Environment.NewLine, aggEx.InnerExceptions.Select(ex => ex.Message));

                    dialogService.ShowError("Markpad", "There was a problem contacting the website. Check the settings and try again.", message);
                    return;
                }

                var newAPIBlogs = new ObservableCollection<FetchedBlogInfo>();

                foreach (var blogInfo in continueParam.Result)
                {
                    newAPIBlogs.Add(new FetchedBlogInfo { Name = blogInfo.blogName, BlogInfo = blogInfo });
                }

                APIBlogs = newAPIBlogs;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void DiscoverAddress()
        {
            if (Uri.IsWellFormedUriString(CurrentBlog.WebAPI, UriKind.Absolute))
                DiscoverAddress(CurrentBlog.WebAPI);
            else
            {
                dialogService.ShowWarning("Enter blog address", "Enter your blog address to discover the MetaWeblog Uri", null);
            }
        }

        private void DiscoverAddress(string webAPI)
        {
            //TODO Show busy
            var baseUri = new Uri(webAPI, UriKind.Absolute);
            var requestUri = new Uri(baseUri, "rsd.xml");
            var rsdFileRequest = WebRequest.Create(requestUri);
            Task.Factory.FromAsync<WebResponse>(rsdFileRequest.BeginGetResponse, rsdFileRequest.EndGetResponse, null)
                .ContinueWith<bool>(ReadRsdXmlFile)
                .ContinueWith<bool>(c => DiscoverIfNescessary(c, webAPI))
                .ContinueWith(HandleResult)
                .ContinueWith(HideBusy);
        }

        private bool ReadRsdXmlFile(Task<WebResponse> c)
        {
            var webResponse = (HttpWebResponse)c.Result;
            if (webResponse.StatusCode == HttpStatusCode.OK)
            {
                using (var responseStream = c.Result.GetResponseStream())
                using (var streamReader = new StreamReader(responseStream))
                {
                    var response = streamReader.ReadToEnd();
                    XElement xElement = XDocument.Parse(response)
                        .Element(XName.Get("rsd", "http://tales.phrasewise.com/rfc/rsd"));
                    var api = xElement
                        .Element("service")
                        .Element("apis")
                        .Elements("api")
                        .SingleOrDefault(a => a.Attribute("name").Value.ToLower() == "metaweblog");

                    if (api != null)
                    {
                        Execute.OnUIThread(() => CurrentBlog.WebAPI = api.Attribute("apiLink").Value);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool DiscoverIfNescessary(Task<bool> taskToContinue, string webAPI)
        {
            if (taskToContinue.IsFaulted || !taskToContinue.Result)
            {
                Trace.WriteLine(string.Format("Tsd.xml does not exist, trying to discover via link. Error was {0}", taskToContinue.Exception), "INFO");

                var webRequest = WebRequest.Create(webAPI);
                return Task.Factory.FromAsync<WebResponse>(webRequest.BeginGetResponse, webRequest.EndGetResponse, null)
                    .ContinueWith<bool>(c =>
                    {
                        using (var streamReader = new StreamReader(c.Result.GetResponseStream()))
                        {
                            var response = streamReader.ReadToEnd();
                            var link = Regex.Match(response, "(?<link>\\<link .*?type=\"application/rsd\\+xml\".*?/\\>)", RegexOptions.IgnoreCase);
                            var @group = link.Groups["link"];
                            if (!@group.Success) return false;
                            var rsdLocation = Regex.Match(@group.Value, "href=\"(?<link>.*?)\"");
                            if (!rsdLocation.Groups["link"].Success) return false;
                            var rsdUri = new Uri(rsdLocation.Groups["link"].Value, UriKind.RelativeOrAbsolute);
                            if (!rsdUri.IsAbsoluteUri)
                                rsdUri = new Uri(new Uri(webAPI, UriKind.Absolute), rsdUri);

                            webRequest = WebRequest.Create(rsdUri);
                            using (var rsdReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                            {
                                var rds = rsdReader.ReadToEnd();
                                var apiLinkMatch = Regex.Match(rds, "(?<apiLink>\\<api .*?name=\"MetaWeblog\".*?/\\>)", RegexOptions.IgnoreCase);
                                var apiLinkGroup = apiLinkMatch.Groups["apiLink"];
                                if (!apiLinkGroup.Success) return false;
                                var apiLink = Regex.Match(apiLinkGroup.Value, "apiLink=\"(?<apiLink>.*?)\"");
                                var apiLinkAttributeGroup = apiLink.Groups["apiLink"];
                                if (!apiLinkAttributeGroup.Success) return false;
                                Execute.OnUIThread(() => CurrentBlog.WebAPI = apiLinkAttributeGroup.Value);
                                return true;
                            }
                        }
                    })
                    .Result; //Not ideal, but otherwise code gets too messy

            }

            return taskToContinue.Result;
        }

        private void HandleResult(Task<bool> obj)
        {
            if (obj.IsFaulted || !obj.Result)
            {
                dialogService.ShowError("Discovery failed", "Make sure you have a rsd.xml in the root of your blog, or put a link to it on your blog homepage head",
                    obj.IsFaulted ? GetErrorMessage(obj.Exception) : null);
            }
        }

        private static string GetErrorMessage(Exception ex)
        {
            return ((AggregateException)ex).Flatten().InnerException.Message;
        }

        private void HideBusy(Task obj)
        {
        }
    }
}
