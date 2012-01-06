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

        public KeyValuePair<string, Post> CurrentPost
        {
            get
            {
                var post = _settings.Get<Post>("CurrentPost");
             
                return new KeyValuePair<string, Post>(post.title, post);
            }
            set
            {
                _settings.Set("CurrentPost", value.Value);
            }
        }

        public Dictionary<string, Post> Posts { get; set; }

        public void Fetch()
        {
            var proxy = XmlRpcProxyGen.Create<IMetaWeblog>();
            ((IXmlRpcProxy)proxy).Url = this.BlogUrl;
            
            this.Posts = proxy.GetRecentPosts("0", this.Username, this.Password, 100).ToDictionary(p => p.title);
        }
    }
}
