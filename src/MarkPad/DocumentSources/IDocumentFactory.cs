using System.Threading.Tasks;
using MarkPad.Plugins;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources
{
    public interface IDocumentFactory
    {
        IMarkpadDocument NewDocument();
        IMarkpadDocument NewDocument(string initalText);
        IMarkpadDocument NewDocument(string initalText, string title);
        IMarkpadDocument CreateHelpDocument(string title, string content);
        Task<IMarkpadDocument> OpenDocument(string path);
        Task<IMarkpadDocument> PublishDocument(string postId, IMarkpadDocument document);
        Task<IMarkpadDocument> SaveDocumentAs(IMarkpadDocument document);
        Task<IMarkpadDocument> OpenBlogPost(BlogSetting blog, string id, string name);
    }
}