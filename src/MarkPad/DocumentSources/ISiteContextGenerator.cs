using MarkPad.DocumentSources.WebSources;
using MarkPad.Plugins;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources
{
    public interface ISiteContextGenerator
    {
        ISiteContext GetContext(string fileName);
        WebSiteContext GetWebContext(BlogSetting blog);
    }
}