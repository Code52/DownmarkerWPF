using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Caliburn.Micro;
using CookComputing.XmlRpc;
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

        public void FetchBlogs()
        {
            SelectedAPIBlog = null;
            try
            {
                var proxy = XmlRpcProxyGen.Create<IMetaWeblog>();
                ((IXmlRpcProxy)proxy).Url = CurrentBlog.WebAPI;

                var blogs = proxy.GetUsersBlogs("MarkPad", CurrentBlog.Username, CurrentBlog.Password);

                APIBlogs = new ObservableCollection<FetchedBlogInfo>();

                foreach (var blogInfo in blogs)
                {
                    APIBlogs.Add(new FetchedBlogInfo { Name = blogInfo.blogName, BlogInfo = blogInfo });
                }
            }
            catch (WebException ex)
            {
                dialogService.ShowError("Fetch Failed", ex.Message, "");
            }
            catch (XmlRpcException ex)
            {
                dialogService.ShowError("Fetch Failed", ex.Message, "");
            }
            catch (XmlRpcFaultException ex)
            {
                dialogService.ShowError("Fetch Failed", ex.Message, "");
            }
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


            //WebRequest webRequest = WebRequest.Create(webAPI);
            //Task.Factory.FromAsync<WebResponse>(webRequest.BeginGetResponse, webRequest.EndGetResponse, null)
            //    .ContinueWith(c=>
            //                  {
            //                      var response = new StreamReader(c.Result.GetResponseStream()).ReadToEnd()

            //                  })
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
                WebRequest webRequest = WebRequest.Create(webAPI);
                Task.Factory.FromAsync<WebResponse>(webRequest.BeginGetResponse, webRequest.EndGetResponse, null)
                    .ContinueWith(c =>
                    {
                        var response = new StreamReader(c.Result.GetResponseStream()).ReadToEnd();

                        //TODO parse page to get <link rel="EditURI" type="application/rsd+xml" href="<metawebloguri>" /> from page
                    })
                    .Wait(); //Not ideal, but otherwise code gets too messy
            }

            return taskToContinue.Result;
        }

        private void HandleResult(Task<bool> obj)
        {
            if (obj.IsFaulted || !obj.Result)
                dialogService.ShowError("Discovery failed", "Make sure you have a rsd.xml in the root of your blog, or put a link to it on your blog homepage head",
                    obj.IsFaulted ? GetErrorMessage(obj.Exception) : null);
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
