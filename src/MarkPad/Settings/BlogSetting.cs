using System;
namespace MarkPad.Settings
{
    [Serializable]
    public class BlogSetting
    {
        public string BlogName { get; set; }
        public string WebAPI { get; set; }
        public string WebAPIUser { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}