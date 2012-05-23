using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.Framework;
using MarkPad.Services.Interfaces;
using MarkPad.Services.Metaweblog;
using MarkPad.Services.Settings;

namespace MarkPad.OpenFromWeb
{
    public class OpenFromWebViewModel : Screen
    {
        private readonly IDialogService dialogService;
        private readonly Func<string, IMetaWeblogService> getMetaWeblog;
        private readonly ITaskSchedulerFactory taskScheduler;

        public OpenFromWebViewModel(
            IDialogService dialogService, 
            Func<string, IMetaWeblogService> getMetaWeblog,
            ITaskSchedulerFactory taskScheduler )
        {
            this.dialogService = dialogService;
            this.getMetaWeblog = getMetaWeblog;
            this.taskScheduler = taskScheduler;
        }

        public void InitializeBlogs(List<BlogSetting> blogs)
        {
            Blogs = blogs;
            SelectedBlog = blogs.FirstOrDefault();
        }

        public List<BlogSetting> Blogs { get; private set; }

        public BlogSetting SelectedBlog { get; set; }

        public Post SelectedPost { get; set; }

        public Entry CurrentPost
        {
            get
            {
                return new Entry { Key = SelectedPost.title, Value = SelectedPost };
            }
            set
            {
                SelectedPost = value.Value;
            }
        }

        public ObservableCollection<Entry> Posts { get; private set; }

        public bool CanFetch { get { return SelectedBlog != null; } }

        protected override void OnActivate()
        {
            base.OnActivate();
            if (CanFetch)
            {
                Fetch();
            }
        }

        public bool CanContinue
        {
            get { return !string.IsNullOrWhiteSpace(CurrentPost.Key); }
        }

        public void Continue()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }

        public Task Fetch()
        {
            Posts = new ObservableCollection<Entry>();

            var proxy = getMetaWeblog(SelectedBlog.WebAPI);

            return proxy.GetRecentPostsAsync(SelectedBlog, 100)
                .ContinueWith(UpdateBlogPosts, taskScheduler.FromCurrentSynchronisationContext())
                .ContinueWith(HandleFetchError);
        }

        private void UpdateBlogPosts(Task<Post[]> t)
        {
            t.PropagateExceptions();

            foreach (var p in t.Result)
            {
                Posts.Add(new Entry { Key = p.title, Value = p });
            }

            var topPost = Posts.FirstOrDefault();
            if (topPost != null)
                CurrentPost = topPost;
        }

        private void HandleFetchError(Task t)
        {
            if (!t.IsFaulted)
                return;

            dialogService.ShowError("Markpad", "There was a problem contacting the website. Check the settings and try again.", t.Exception.GetErrorMessage());
        }
    }
}
