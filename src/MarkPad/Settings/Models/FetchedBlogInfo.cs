using Caliburn.Micro;

namespace MarkPad.Settings.Models
{
    public class FetchedBlogInfo : PropertyChangedBase
    {
        public string Name { get; set; }
        public BlogInfo BlogInfo { get; set; }
    }
}
