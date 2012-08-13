using System.Threading.Tasks;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.MetaWeblog.Service
{
    public interface IMetaWeblogService
    {
        string NewPost(BlogSetting settings, Post newpost, bool publish);
        Post GetPost(string postid, BlogSetting settings);
        void EditPost(string postid, BlogSetting settings, Post newpost, bool publish);
        Task<Post[]> GetRecentPostsAsync(BlogSetting selectedBlog, int count);
        Task<BlogInfo[]> GetUsersBlogsAsync(BlogSetting setting);
        bool DeletePost(string postid, BlogSetting settings);
        MediaObjectInfo NewMediaObject(BlogSetting settings, MediaObject mediaObject);
    }
}
