using System.Collections.Generic;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.MetaWeblog
{
    public interface IBlogService
    {
        bool ConfigureNewBlog(string featureName);
        BlogSetting AddBlog();
        bool EditBlog(BlogSetting currentBlog);
        void Remove(BlogSetting currentBlog);
        List<BlogSetting> GetBlogs();
    }
}