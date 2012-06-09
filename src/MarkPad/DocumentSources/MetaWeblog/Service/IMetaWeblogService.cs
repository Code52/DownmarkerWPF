using System.Threading.Tasks;
using MarkPad.Services.Settings;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.MetaWeblog.Service
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
