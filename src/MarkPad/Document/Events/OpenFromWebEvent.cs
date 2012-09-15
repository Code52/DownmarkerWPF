using MarkPad.Settings.Models;

namespace MarkPad.Document.Events
{
    public class OpenFromWebEvent
    {
        public OpenFromWebEvent(string id, string name, BlogSetting blog)
        {
            Id = id;
            Name = name;
            Blog = blog;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public BlogSetting Blog { get; private set; }
    }
}