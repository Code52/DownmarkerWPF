using System.Threading.Tasks;
using MarkPad.DocumentSources.NewDocument;
using MarkPad.Infrastructure;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources
{
    public class HelpDocument : NewMarkpadDocument
    {
        public HelpDocument(string title, string helpText, IDocumentFactory documentFactory, IFileSystem fileSystem) 
            : base(fileSystem, documentFactory, title, helpText)
        { }

        public override Task<IMarkpadDocument> Save()
        {
            return SaveAs();
        }
    }
}