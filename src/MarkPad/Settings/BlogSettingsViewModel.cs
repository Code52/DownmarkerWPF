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
using MarkPad.Framework;
using MarkPad.Metaweblog;
using MarkPad.Services.Interfaces;

namespace MarkPad.Settings
{
    public class BlogSettingsViewModel : Screen
    {
        private const string RsdNamespace = "http://archipelago.phrasewise.com/rsd";
        
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

            proxy
                .GetUsersBlogsAsync("MarkPad", CurrentBlog.Username, CurrentBlog.Password)
                .ContinueWith(UpdateBlogList, TaskScheduler.FromCurrentSynchronizationContext())
                .ContinueWith(HandleFetchError);
        }

        private void UpdateBlogList(Task<BlogInfo[]> t)
        {
            t.PropagateExceptions();

            var newAPIBlogs = new ObservableCollection<FetchedBlogInfo>();

            foreach (var blogInfo in t.Result)
            {
                newAPIBlogs.Add(new FetchedBlogInfo { Name = blogInfo.blogName, BlogInfo = blogInfo });
            }

            APIBlogs = newAPIBlogs;
        }

        private void HandleFetchError(Task t)
        {
            if (!t.IsFaulted)
                return;

            dialogService.ShowError("Markpad", "There was a problem contacting the website. Check the settings and try again.", t.Exception.GetErrorMessage());
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

            // Kick off the async discovery workflow
            Task.Factory.FromAsync<WebResponse>(rsdFileRequest.BeginGetResponse, rsdFileRequest.EndGetResponse, null)
                .ContinueWith<bool>(ProcessRsdResponse)
                .ContinueWith(c => DiscoverRsdLinkIfNescessary(c, webAPI)).Unwrap()
                .ContinueWith(HandleResult)
                .ContinueWith(HideBusy);
        }

        private bool ProcessRsdResponse(Task<WebResponse> c)
        {
            var webResponse = (HttpWebResponse)c.Result;

            if(webResponse.StatusCode == HttpStatusCode.OK)
            {
                using(var responseStream = webResponse.GetResponseStream())
                {
                    var apiElement = XDocument.Load(responseStream)
                        .Element(XName.Get("rsd", RsdNamespace))
                        .Element(XName.Get("service", RsdNamespace))
                        .Element(XName.Get("apis", RsdNamespace))
                        .Elements(XName.Get("api", RsdNamespace))
                        .SingleOrDefault(e => e.Attribute("name").Value.ToLower() == "metaweblog");

                    if(apiElement != null)
                    {
                        Execute.OnUIThread(() => CurrentBlog.WebAPI = apiElement.Attribute("apiLink").Value);
                        return true;
                    }
                }
            }

            return false;
        }

        private Task<bool> DiscoverRsdLinkIfNescessary(Task<bool> taskToContinue, string webAPI)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            
            // If the original
            if(taskToContinue.IsFaulted || !taskToContinue.Result)
            {
                Trace.WriteLine(string.Format("Rsd.xml does not exist, trying to discover via link. Error was {0}", taskToContinue.Exception), "INFO");

                // Build a request to retrieve the contents of the specified URL directly
                var directWebAPIRequest = WebRequest.Create(webAPI);
                Task<WebResponse> webAPIRequestTask = Task.Factory.FromAsync<WebResponse>(directWebAPIRequest.BeginGetResponse, directWebAPIRequest.EndGetResponse, null);

                // Add a continuation that will only execute if the request succeeds and proceses the response to look for a <link> to the RSD
                webAPIRequestTask.ContinueWith(webAPIRequestAntecedent =>
                    {
                        WebResponse webAPIResponse = webAPIRequestAntecedent.Result;

                        try
                        {
                            using(var streamReader = new StreamReader(webAPIResponse.GetResponseStream()))
                            {
                                var response = streamReader.ReadToEnd();
                                var link = Regex.Match(response, "(?<link>\\<link .*?type=\"application/rsd\\+xml\".*?/\\>)", RegexOptions.IgnoreCase);
                                var @group = link.Groups["link"];

                                if(!@group.Success)
                                {
                                    taskCompletionSource.SetResult(false);
                                }
                                else
                                {
                                    var rsdLocation = Regex.Match(@group.Value, "href=\"(?<link>.*?)\"");
                                    if(!rsdLocation.Groups["link"].Success)
                                    {
                                        taskCompletionSource.SetResult(false);
                                    }
                                    else
                                    {
                                        var rsdUri = new Uri(rsdLocation.Groups["link"].Value, UriKind.RelativeOrAbsolute);
                                        if(!rsdUri.IsAbsoluteUri)
                                            rsdUri = new Uri(new Uri(webAPI, UriKind.Absolute), rsdUri);

                                        WebRequest rdsWebRequest = WebRequest.Create(rsdUri);

                                        Task<WebResponse> rdsWebRequestTask = Task.Factory.FromAsync<WebResponse>(rdsWebRequest.BeginGetResponse, rdsWebRequest.EndGetResponse, null);

                                        // Add a continuation that will only execute if the request succeeds and continues processing the RSD
                                        rdsWebRequestTask.ContinueWith(rdsWebRequestAntecedent =>
                                                {
                                                    WebResponse rdsWebResponse = rdsWebRequestAntecedent.Result;

                                                    try
                                                    {
                                                        taskCompletionSource.SetResult(ProcessRsdResponse(rdsWebRequestAntecedent));
                                                    }
                                                    finally
                                                    {
                                                        // No matter what happens, make sure we clean up the web response
                                                        rdsWebResponse.Close();
                                                    }
                                                },
                                                TaskContinuationOptions.NotOnFaulted);

                                        // Add a continuation that will only execute if the request faults and propagates the exception via the TCS
                                        rdsWebRequestTask.ContinueWith(rdsWebRequestAntecdent =>
                                            {
                                                taskCompletionSource.SetException(rdsWebRequestAntecdent.Exception);
                                            },
                                            TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            // No matter what happens, make sure we clean up the web response
                            webAPIResponse.Close();
                        }
                    },
                    TaskContinuationOptions.NotOnFaulted);

                // Add a continuation that will only fire if the request faults and propagates the exception via the TCS
                webAPIRequestTask.ContinueWith(webAPIRequestAntecedent =>
                    {
                        taskCompletionSource.SetException(webAPIRequestAntecedent.Exception);
                    },
                    TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);
            }
            else
            {
                taskCompletionSource.SetResult(true);
            }

            return taskCompletionSource.Task;
        }

        private void HandleResult(Task<bool> obj)
        {
            if (obj.IsFaulted || !obj.Result)
            {
                dialogService.ShowError("Discovery failed", "Make sure you have a rsd.xml in the root of your blog, or put a link to it on your blog homepage head",
                    obj.IsFaulted ? obj.Exception.GetErrorMessage() : null);
            }
        }

        private void HideBusy(Task obj)
        {
        }
    }
}
