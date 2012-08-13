using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Settings.Models;

namespace MarkPad.Events
{
    public class OpenFromWebEvent
    {
        public OpenFromWebEvent(Post post, BlogSetting selectedBlog)
        {
            SelectedBlog = selectedBlog;
            Post = post;
        }

        public Post Post { get; private set; }
        public BlogSetting SelectedBlog { get; private set; }
    }
}