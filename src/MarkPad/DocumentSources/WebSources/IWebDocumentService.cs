using System.Threading.Tasks;
using MarkPad.DocumentSources.GitHub;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.WebSources
{
    public interface IWebDocumentService
    {
        Task DeleteDocument(BlogSetting blog, Post post);
        Task<string> SaveDocument(BlogSetting blog, WebDocument document);
        Task<string> GetDocumentContent(BlogSetting blog, string id);
    }
}