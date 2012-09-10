using System.Threading.Tasks;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.MetaWeblog.Service
{
    public interface IMetaWeblogService
    {
        string NewPost(BlogSetting settings, Post newpost, bool publish);
        Post GetPost(string postid, BlogSetting settings);
        void EditPost(string postid, BlogSetting settings, Post newpost, bool publish);
        bool DeletePost(string postid, BlogSetting blog);
        MediaObjectInfo NewMediaObject(BlogSetting blog, MediaObject mediaObject);
        Task<Post> GetPostAsync(string postid, BlogSetting settings);
        Task<Post[]> GetRecentPostsAsync(BlogSetting selectedBlog, int count);
        Task<BlogInfo[]> GetUsersBlogsAsync(BlogSetting setting);
        Task<bool> DeletePostAsync(string postid, BlogSetting blog);
        Task<MediaObjectInfo> NewMediaObjectAsync(BlogSetting blog, MediaObject mediaObject);
        Task EditPostAsync(string postid, BlogSetting blog, Post post, bool publish);
    }
}
