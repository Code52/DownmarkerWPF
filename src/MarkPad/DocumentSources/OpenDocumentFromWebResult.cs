using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources
{
    public class OpenDocumentFromWebResult
    {
        public bool? Success { get; set; }
        public Post SelectedPost { get; set; }
        public BlogSetting SelectedBlog { get; set; }
    }
}