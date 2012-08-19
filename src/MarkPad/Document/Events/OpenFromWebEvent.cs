using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Settings.Models;

namespace MarkPad.Document.Events
{
    public class OpenFromWebEvent
    {
        public OpenFromWebEvent(Post post, BlogSetting blog)
        {
            Post = post;
            Blog = blog;
        }

        public Post Post { get; private set; }
        public BlogSetting Blog { get; private set; }
    }
}