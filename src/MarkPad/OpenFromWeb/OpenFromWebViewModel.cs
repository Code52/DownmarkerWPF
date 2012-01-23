using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.Framework;
using MarkPad.Metaweblog;
using MarkPad.Services.Interfaces;
using MarkPad.Settings;

namespace MarkPad.OpenFromWeb
{
    public class OpenFromWebViewModel : Screen
    {
        private readonly ISettingsService settings;
        private readonly IDialogService dialogService;

        public OpenFromWebViewModel(ISettingsService settings, IDialogService dialogService)
        {
            this.settings = settings;
            this.dialogService = dialogService;
        }

        public void InitializeBlogs(List<BlogSetting> blogs)
        {
            Blogs = blogs;
            SelectedBlog = blogs[0];
        }

        public List<BlogSetting> Blogs { get; private set; }

        public BlogSetting SelectedBlog { get; set; }

        public Entry CurrentPost
        {
            get
            {
                var post = settings.Get<Post>("CurrentPost");

                return new Entry { Key = post.title, Value = post };
            }
            set
            {
                settings.Set("CurrentPost", value.Value);
            }
        }

        public ObservableCollection<Entry> Posts { get; private set; }

        public void Fetch()
        {
            Posts = new ObservableCollection<Entry>();

            var proxy = new MetaWeblog(this.SelectedBlog.WebAPI);

            proxy
                .GetRecentPostsAsync(SelectedBlog.BlogInfo.blogid, SelectedBlog.Username, SelectedBlog.Password, 100)
                .ContinueWith(UpdateBlogPosts, TaskScheduler.FromCurrentSynchronizationContext())
                .ContinueWith(HandleFetchError);
        }

        private void UpdateBlogPosts(Task<Post[]> t)
        {
            t.PropagateExceptions();

            foreach (var p in t.Result)
            {
                Posts.Add(new Entry { Key = p.title, Value = p });
            }
        }

        private void HandleFetchError(Task t)
        {
            if (!t.IsFaulted)
                return;

            dialogService.ShowError("Markpad", "There was a problem contacting the website. Check the settings and try again.", t.Exception.GetErrorMessage());
        }
    }
}
