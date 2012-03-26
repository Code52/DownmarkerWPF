using System.Threading.Tasks;
using MarkPad.Services.Metaweblog;
using MarkPad.Services.Settings;

namespace MarkPad.Services.Interfaces
{
    public interface IMetaWeblogService
    {
        string NewPost(BlogSetting settings, Post newpost, bool b);
        Post GetPost(string postid, BlogSetting settings);
        void EditPost(string postid, BlogSetting settings, Post newpost, bool b);
        Task<Post[]> GetRecentPostsAsync(BlogSetting selectedBlog, int count);
        Task<BlogInfo[]> GetUsersBlogsAsync(BlogSetting setting);
    }
}
