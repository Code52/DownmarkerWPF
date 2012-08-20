using System.Data.Entity;
using MetaWebLogSite.Models;

namespace MetaWebLogSite.XmlRpc
{
    public class BlogDataContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogPost> Posts { get; set; }
        public DbSet<BlogCategory> Categories { get; set; }
        public DbSet<BlogMediaObject> MediaObjects { get; set; }
    }
}