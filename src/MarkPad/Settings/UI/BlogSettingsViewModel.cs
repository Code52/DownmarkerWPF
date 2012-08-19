using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.DocumentSources.MetaWeblog.Service.Rsd;
using MarkPad.Framework;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Settings.Models;

namespace MarkPad.Settings.UI
{
    public class BlogSettingsViewModel : Screen
    {
        readonly IDialogService dialogService;
        readonly Func<string, IMetaWeblogService> getMetaWeblog;
        private readonly IRsdService discoveryService;

        public BlogSettingsViewModel(IDialogService dialogService, Func<string, IMetaWeblogService> getMetaWeblog, IRsdService discoveryService)
        {
            this.dialogService = dialogService;
            this.getMetaWeblog = getMetaWeblog;
            this.discoveryService = discoveryService;

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

            var proxy = getMetaWeblog(CurrentBlog.WebAPI);

            APIBlogs = new ObservableCollection<FetchedBlogInfo>();

            proxy
                .GetUsersBlogsAsync(CurrentBlog)
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
            var startAddress = CurrentBlog.WebAPI ?? string.Empty;

            if (!startAddress.StartsWith("http://"))
                startAddress = "http://" + startAddress;

            if (Uri.IsWellFormedUriString(startAddress, UriKind.Absolute))
            {
                DiscoveringAddress = true;
                discoveryService.DiscoverAddress(startAddress)
                    .ContinueWith(t =>
                    {
                        if (t.Result.Success)
                            CurrentBlog.WebAPI = t.Result.MetaWebLogApiLink;
                        else
                        {
                            const string errorText = "Make sure you have a rsd.xml in the root of your blog, or put a link to it on your blog homepage head";
                            dialogService.ShowError("Discovery failed", errorText, t.Result.FailMessage);
                        }
                        DiscoveringAddress = false;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                dialogService.ShowWarning("Enter blog address", "Enter your blog address to discover the MetaWeblog Uri", null);
            }
        }

        public bool DiscoveringAddress { get; private set; }
    }
}
