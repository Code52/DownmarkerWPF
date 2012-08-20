using System;

namespace MetaWebLogSite.Models
{
    public class BlogPost
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public string[] Categories { get; set; }
    }

    public class Blog
    {
        public int Id { get; set; }
    }

    public class BlogMediaObject
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public byte[] Bits { get; set; }
    }
}
