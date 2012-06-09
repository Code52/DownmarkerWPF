using System;

namespace MarkPad.DocumentSources.MetaWeblog.Service
{
    [Serializable]
    public struct CategoryInfo
    {
        public string description;
        public string htmlUrl;
        public string rssUrl;
        public string title;
        public string categoryid;
    }
}
