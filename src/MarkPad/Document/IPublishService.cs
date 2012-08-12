using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Settings.Models;

namespace MarkPad.Document
{
    public interface IPublishService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="postid"></param>
        /// <param name="postTitle"></param>
        /// <param name="displayname"></param>
        /// <param name="categories"></param>
        /// <param name="blog"></param>
        /// <param name="content"></param>
        /// <returns>Null when publish fails</returns>
        Post? Publish(string postid, string postTitle, string displayname, string[] categories, BlogSetting blog,
                      string content);
    }
}