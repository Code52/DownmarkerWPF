using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace MarkPad.Contracts
{
	public interface IDocumentView
	{
	    TextView TextView { get; }
	    TextDocument Document { get; }
	}
}
