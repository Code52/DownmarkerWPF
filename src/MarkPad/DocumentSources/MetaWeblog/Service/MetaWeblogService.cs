using System.Threading.Tasks;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.MetaWeblog.Service
{
    public class MetaWeblogService : IMetaWeblogService
    {
        readonly MetaWeblog proxy;

        public MetaWeblogService(string url)
        {
            proxy = new MetaWeblog(url);
        }

        public string NewPost(BlogSetting settings, Post newpost, bool publish)
        {
            return proxy.NewPost(settings.BlogInfo.blogid, settings.Username, settings.Password, newpost, publish);
        }

        public Post GetPost(string postid, BlogSetting settings)
        {
            return proxy.GetPost(postid, settings.Username, settings.Password);
        }

        public void EditPost(string postid, BlogSetting settings, Post newpost, bool publish)
        {
            proxy.EditPost(postid, settings.Username, settings.Password, newpost, publish);
        }

        public bool DeletePost(string postid, BlogSetting blog)
        {
            return proxy.DeletePost(string.Empty, postid, blog.Username, blog.Password, false);
        }

        public MediaObjectInfo NewMediaObject(BlogSetting blog, MediaObject mediaObject)
        {
            return proxy.NewMediaObject(blog.BlogInfo.blogid, blog.Username, blog.Password, mediaObject);
        }

        public Task<Post> GetPostAsync(string postid, BlogSetting settings)
        {
            return proxy.GetPostAsync(postid, settings.Username, settings.Password);
        }

        public Task<Post[]> GetRecentPostsAsync(BlogSetting settings, int i)
        {
            return proxy.GetRecentPostsAsync(settings.BlogInfo.blogid, settings.Username, settings.Password, i);
        }

        public Task<BlogInfo[]> GetUsersBlogsAsync(BlogSetting settings)
        {
            return proxy.GetUsersBlogsAsync("MarkPad", settings.Username, settings.Password);
        }

        public Task<bool> DeletePostAsync(string postid, BlogSetting blog)
        {
            return proxy.DeletePostAsync(string.Empty, postid, blog.Username, blog.Password, false);
        }

        public Task<MediaObjectInfo> NewMediaObjectAsync(BlogSetting blog, MediaObject mediaObject)
        {
            return proxy.NewMediaObjectAsync(blog.BlogInfo.blogid, blog.Username, blog.Password, mediaObject);
        }
    }
}
