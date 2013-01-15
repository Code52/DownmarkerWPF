using System.Threading.Tasks;

namespace MarkPad.DocumentSources
{
    public interface IOpenDocumentFromWeb
    {
        Task<OpenDocumentFromWebResult> Open();
    }
}