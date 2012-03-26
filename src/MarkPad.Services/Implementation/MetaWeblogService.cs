using System.Threading.Tasks;
using MarkPad.Services.Interfaces;
using MarkPad.Services.Metaweblog;
using MarkPad.Services.Settings;

namespace MarkPad.Services.Implementation
{
    public class MetaWeblogService : IMetaWeblogService
    {
        private MetaWeblog proxy;

        public MetaWeblogService(string url)
        {
            proxy = new MetaWeblog(url);
        }

        public string NewPost(string blogid, string username, string password, Post newpost, bool b)
        {
            return proxy.NewPost(blogid, username, password, newpost, b);
        }

        public Post GetPost(string postid, string username, string password)
        {
            return proxy.GetPost(postid, username, password);
        }

        public void EditPost(string postid, string username, string password, Post newpost, bool b)
        {
            proxy.EditPost(postid, username, password, newpost, b);
        }

        public Task<Post[]> GetRecentPostsAsync(string blogid, string username, string password, int i)
        {
            return proxy.GetRecentPostsAsync(blogid, username, password, i);
        }

        public Task<BlogInfo[]> GetUsersBlogsAsync(string markpad, string username, string password)
        {
            return proxy.GetUsersBlogsAsync(markpad, username, password);
        }
    }
}
