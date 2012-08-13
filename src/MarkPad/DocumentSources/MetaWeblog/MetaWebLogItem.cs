using System;
using Caliburn.Micro;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.MetaWeblog
{
    public class MetaWebLogItem : SiteItemBase
    {
        readonly Func<string, IMetaWeblogService> getMetaWeblog;
        readonly BlogSetting blog;
        Post post;

        public MetaWebLogItem(Func<string, IMetaWeblogService> getMetaWeblog, IEventAggregator eventAggregator, Post post, BlogSetting blog) : 
            base(eventAggregator)
        {
            this.getMetaWeblog = getMetaWeblog;
            this.post = post;
            this.blog = blog;
            Name = post.title;
        }

        public Post Post
        {
            get { return post; }
        }

        public override void CommitRename()
        {
            post.title = Name;
            getMetaWeblog(blog.WebAPI).EditPost((string)post.postid, blog, post, true);
        }

        public override void UndoRename()
        {
            Name = post.title;
        }

        public override void Delete()
        {
            getMetaWeblog(blog.WebAPI).DeletePost((string)post.postid, blog);
        }
    }
}