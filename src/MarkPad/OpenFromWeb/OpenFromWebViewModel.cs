using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Caliburn.Micro;
using CookComputing.XmlRpc;
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
            var proxy = new MetaWeblog(this.SelectedBlog.WebAPI);

            try
            {
                var posts = proxy.GetRecentPosts(SelectedBlog.BlogInfo.blogid, SelectedBlog.Username, SelectedBlog.Password, 100);

                Posts = new ObservableCollection<Entry>();

                foreach (var p in posts)
                {
                    Posts.Add(new Entry { Key = p.title, Value = p });
                }
            }
            catch (WebException ex)
            {
                dialogService.ShowError("Error Fetching Posts", ex.Message, "");
            }
            catch (XmlRpcException ex)
            {
                dialogService.ShowError("Error Fetching Posts", ex.Message, "");
            }
            catch (XmlRpcFaultException ex)
            {
                dialogService.ShowError("Error Fetching Posts", ex.Message, "");
            }
        }
    }
}
