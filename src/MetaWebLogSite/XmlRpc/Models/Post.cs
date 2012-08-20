using System;
using CookComputing.XmlRpc;

// ReSharper disable InconsistentNaming
namespace MetaWebLogSite.XmlRpc.Models
{
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct Post
    {
        public DateTime dateCreated;
        public string description;
        public string title;
        public string[] categories;
        public string permalink;
        public string postid;
        public string userid;
        public string wp_slug;
        public string format;
    }
}
// ReSharper restore InconsistentNaming
