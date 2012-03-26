using System.Threading.Tasks;
using MarkPad.Services.Metaweblog;
using MarkPad.Services.Settings;

namespace MarkPad.Services.Interfaces
{
    public interface IMetaWeblogService
    {
        string NewPost(string blogid, string username, string password, Post newpost, bool b);
        Post GetPost(string postid, string username, string password);
        void EditPost(string postid, string username, string password, Post newpost, bool b);
        Task<Post[]> GetRecentPostsAsync(string blogid, string username, string password, int i);
        Task<BlogInfo[]> GetUsersBlogsAsync(string markpad, string username, string password);
    }
}
