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
        private readonly ISettingsService _settings;
        private readonly IDialogService dialogService;

        public OpenFromWebViewModel(ISettingsService settings, IDialogService dialogService)
        {
            this._settings = settings;
            this.dialogService = dialogService;
        }

        public void InitializeBlogs(List<BlogSetting> blogs)
        {
            this.Blogs = blogs;
            SelectedBlog = blogs[0];
        }

        public List<BlogSetting> Blogs { get; set; }

        public BlogSetting SelectedBlog { get; set; }

        public Entry CurrentPost
        {
            get
            {
                var post = _settings.Get<Post>("CurrentPost");

                return new Entry { Key = post.title, Value = post };
            }
            set
            {
                _settings.Set("CurrentPost", value.Value);
            }
        }

        public ObservableCollection<Entry> Posts { get; set; }

        public void Fetch()
        {
            var proxy = XmlRpcProxyGen.Create<IMetaWeblog>();
            ((IXmlRpcProxy)proxy).Url = this.SelectedBlog.WebAPI;

            try
            {
                var posts = proxy.GetRecentPosts(this.SelectedBlog.BlogInfo.blogid, this.SelectedBlog.Username, this.SelectedBlog.Password, 100);

                this.Posts = new ObservableCollection<Entry>();

                foreach (var p in posts)
                {
                    this.Posts.Add(new Entry { Key = p.title, Value = p });
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

    public class Entry
    {
        public string Key { get; set; }
        public Post Value { get; set; }
    }
}
