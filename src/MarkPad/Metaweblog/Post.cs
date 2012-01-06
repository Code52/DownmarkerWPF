using System;
using CookComputing.XmlRpc;

namespace MarkPad.Metaweblog
{
    [Serializable]
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct Post
    {
        public DateTime dateCreated;
        public string description;
        public string title;
        public string[] categories;
        public string permalink;
        public object postid;
        public string userid;
        public string wp_slug;
    }
}