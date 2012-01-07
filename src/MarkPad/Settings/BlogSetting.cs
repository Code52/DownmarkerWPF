using System;
using MarkPad.Metaweblog;

namespace MarkPad.Settings
{
    [Serializable]
    public class BlogSetting
    {
        public string BlogName { get; set; }
        public string WebAPI { get; set; }
        public BlogInfo BlogInfo { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Language { get; set; }
    }
}