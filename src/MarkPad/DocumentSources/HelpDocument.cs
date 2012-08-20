using System.Drawing;
using System.Threading.Tasks;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources
{
    public class HelpDocument : NewMarkpadDocument
    {
        public HelpDocument(string title, string helpText, IDocumentFactory documentFactory) 
            : base(documentFactory, title, helpText)
        { }

        public override Task<IMarkpadDocument> Save()
        {
            return SaveAs();
        }
    }
}