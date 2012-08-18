using System.Threading.Tasks;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources
{
    public class HelpDocument : MarkpadDocumentBase
    {
        public HelpDocument(string title, string helpText, IDocumentFactory documentFactory) 
            : base(title, helpText, null, documentFactory)
        { }

        public override Task<IMarkpadDocument> Save()
        {
            return SaveAs();
        }
    }
}