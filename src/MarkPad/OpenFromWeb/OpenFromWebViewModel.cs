using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using CookComputing.XmlRpc;
using MarkPad.Metaweblog;
using MarkPad.Services.Interfaces;

namespace MarkPad.OpenFromWeb
{
    public class OpenFromWebViewModel : Screen
    {
        private readonly ISettingsService _settings;

        public OpenFromWebViewModel(ISettingsService settings)
        {
            _settings = settings;
        }

        public string BlogUrl
        {
            get { return _settings.Get<string>("BlogUrl"); }
            set { _settings.Set("BlogUrl", value); }
        }

        public string Username
        {
            get { return _settings.Get<string>("Username"); }
            set { _settings.Set("Username", value); }
        }

        public string Password
        {
            get { return _settings.Get<string>("Password"); }
            set { _settings.Set("Password", value); }
        }

        public Entry CurrentPost
        {
            get
            {
                var post = _settings.Get<Post>("CurrentPost");

                return new Entry {Key = post.title, Value = post};
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
            ((IXmlRpcProxy)proxy).Url = this.BlogUrl;
            
            var posts = proxy.GetRecentPosts("0", this.Username, this.Password, 100);

            this.Posts = new ObservableCollection<Entry>();

            foreach(var p in posts)
            {
                this.Posts.Add(new Entry {Key = p.title, Value = p});
            }
        }
    }

    public class Entry
    {
        public string Key { get; set; }
        public Post Value { get; set; }
    }
}
