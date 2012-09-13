using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.DocumentSources.WebSources;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.MetaWeblog
{
    public class MetaWeblogSiteContext : WebSiteContext
    {
        readonly Func<string, IMetaWeblogService> getMetaWeblog;

        public MetaWeblogSiteContext(
            BlogSetting blog,
            Func<string, IMetaWeblogService> getMetaWeblog,
            IWebDocumentService webDocumentService,
            IEventAggregator eventAggregator) : base(blog, webDocumentService, eventAggregator)
        {
            this.getMetaWeblog = getMetaWeblog;
        }

        protected override Task<Post[]> GetItems()
        {
            return getMetaWeblog(Blog.WebAPI).GetRecentPostsAsync(Blog, 100);            
        }
    }
}