using System.Threading.Tasks;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources
{
    public class NewMarkpadDocument : MarkpadDocumentBase
    {
        public NewMarkpadDocument(IDialogService dialogService, IDocumentFactory documentFactory, string content) : 
            base("New Document", content, null, documentFactory)
        { }

        public override Task<IMarkpadDocument> Save()
        {
            return SaveAs();
        }
    }
}