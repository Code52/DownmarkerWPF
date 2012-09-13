using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.DocumentSources.WebSources;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.GitHub
{
    public class GithubSiteContext : WebSiteContext
    {
        readonly IGithubApi github;

        public GithubSiteContext(BlogSetting blog, 
            IWebDocumentService webDocumentService,
            IGithubApi github,
            IEventAggregator eventAggregator) :
            base(blog, webDocumentService, eventAggregator)
        {
            this.github = github;
        }

        protected override Task<Post[]> GetItems()
        {
            return github.FetchFiles(Blog.Username, Blog.WebAPI, Blog.BlogInfo.blogid, Blog.Token);
        }
    }
}