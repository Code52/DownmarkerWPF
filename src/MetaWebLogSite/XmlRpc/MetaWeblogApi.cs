using System.Linq;
using System.Web;
using CookComputing.XmlRpc;
using MetaWebLogSite.Models;
using MetaWebLogSite.XmlRpc.Models;

namespace MetaWebLogSite.XmlRpc
{
    public class MetaWeblogApi : XmlRpcService
    {
        readonly BlogDataContext dbContext;

        public MetaWeblogApi()
        {
            dbContext = new BlogDataContext();
        }

        [XmlRpcMethod("metaWeblog.newPost")]
        public string NewPost(string blogid, string username, string password, Post post, bool publish)
        {
            var newPost = dbContext.Posts.Add(ToPost(post));
            dbContext.SaveChanges();
            return newPost.Id.ToString();
        }

        BlogPost ToPost(Post post)
        {
            return new BlogPost
            {
                Id = post.postid == null ? 0 : int.Parse(post.postid),
                Title = post.title,
                Description = post.description,
                DateCreated = post.dateCreated
            };
        }

        [XmlRpcMethod("metaWeblog.editPost")]
        public bool EditPost(string postid, string username, string password, Post post, bool publish)
        {
            var newPost = dbContext.Posts.Find(postid);
            newPost.Title = post.title;
            newPost.Categories = post.categories;
            newPost.DateCreated = post.dateCreated;
            newPost.Description = post.description;

            dbContext.SaveChanges();
            return true;
        }

        [XmlRpcMethod("metaWeblog.getPost")]
        public Post GetPost(string postid, string username, string password)
        {
            return ToPostInfo(dbContext.Posts.Find(postid));
        }

        [XmlRpcMethod("metaWeblog.getCategories")]
        public CategoryInfo[] GetCategories(string blogid, string username, string password)
        {
            return dbContext.Categories.Select(c=>new CategoryInfo
            {
                description = c.Description,
                categoryid = c.Id.ToString(), 
                title = c.Title
            }).ToArray();
        }

        [XmlRpcMethod("metaWeblog.getRecentPosts")]
        public Post[] GetRecentPosts(string blogid, string username, string password, int numberOfPosts)
        {
            var recentPosts = dbContext.Posts.ToArray().Select(ToPostInfo).ToArray();
            return recentPosts;
        }

        static Post ToPostInfo(BlogPost p)
        {
            return new Post
            {
                postid = p.Id == null ? null : p.Id.ToString(),
                title = p.Title,
                dateCreated = p.DateCreated,
                description = p.Description
            };
        }

        [XmlRpcMethod("metaWeblog.newMediaObject")]
        public MediaObjectInfo NewMediaObject(string blogid, string username, string password, MediaObject mediaObject)
        {
            dbContext.MediaObjects.Add(new BlogMediaObject
            {
                Name = mediaObject.name,
                Bits = mediaObject.bits,
                TypeName = mediaObject.type
            });

            dbContext.SaveChanges();
            return new MediaObjectInfo();
        }

        [XmlRpcMethod("blogger.deletePost")]
        public bool DeletePost(string key, string postid, string username, string password, bool publish)
        {
            var post = dbContext.Posts.Find(postid);

            dbContext.Posts.Remove(post);
            dbContext.SaveChanges();

            return true;
        }

        [XmlRpcMethod("blogger.getUsersBlogs")]
        public BlogInfo[] GetUsersBlogs(string key, string username, string password)
        {
            return new[] {new BlogInfo
            {
                blogid = "1", 
                blogName = "Sample Blog", 
                url = HttpContext.Current.Request.UserHostAddress
            }};
        }

        [XmlRpcMethod("blogger.getUserInfo")]
        public UserInfo GetUserInfo(string key, string username, string password)
        {
            return new UserInfo();
        }
    }
}