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

        public string NewPost(BlogSetting settings, Post newpost, bool b)
        {
            return proxy.NewPost(settings.BlogInfo.blogid, settings.Username, settings.Password, newpost, b);
        }

        public Post GetPost(string postid, BlogSetting settings)
        {
            return proxy.GetPost(postid, settings.Username, settings.Password);
        }

        public void EditPost(string postid, BlogSetting settings, Post newpost, bool b)
        {
            proxy.EditPost(postid, settings.Username, settings.Password, newpost, b);
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
