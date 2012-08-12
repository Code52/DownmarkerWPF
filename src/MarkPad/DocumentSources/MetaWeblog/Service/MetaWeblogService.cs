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

        public MediaObjectInfo NewMediaObject(BlogSetting settings, MediaObject mediaObject)
        {
            return proxy.NewMediaObject(settings.BlogInfo.blogid, settings.Username, settings.Password, mediaObject);
        }

        public bool DeletePost(string postid, BlogSetting settings)
        {
            return proxy.DeletePost(string.Empty, postid, settings.Username, settings.Password, false);
        }

        public Task<bool> DeletePostAsync(string postid, BlogSetting settings)
        {
            return proxy.DeletePostAsync(string.Empty, postid, settings.Username, settings.Password, false);
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

        public Task<Post[]> GetRecentPostsAsync(BlogSetting settings, int i)
        {
            return proxy.GetRecentPostsAsync(settings.BlogInfo.blogid, settings.Username, settings.Password, i);
        }

        public Task<BlogInfo[]> GetUsersBlogsAsync(BlogSetting settings)
        {
            return proxy.GetUsersBlogsAsync("MarkPad", settings.Username, settings.Password);
        }
    }
}
