using System.Threading.Tasks;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources
{
    public interface IDocumentFactory
    {
        IMarkpadDocument NewDocument();
        IMarkpadDocument NewDocument(string initalText);
        IMarkpadDocument CreateHelpDocument(string title, string content);
        Task<IMarkpadDocument> NewMarkdownFile(string path, string markdownContent);
        Task<IMarkpadDocument> OpenDocument(string path);
        Task<IMarkpadDocument> PublishDocument(string postId, IMarkpadDocument document);
        Task<IMarkpadDocument> OpenFromWeb();
        Task<IMarkpadDocument> SaveDocumentAs(IMarkpadDocument document);
    }
}