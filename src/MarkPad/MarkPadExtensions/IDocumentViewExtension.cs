using MarkPad.Document;
using MarkPad.Services.MarkPadExtensions;

namespace MarkPad.MarkPadExtensions
{
	public interface IDocumentViewExtension : IMarkPadExtension
	{
		void ConnectToDocumentView(DocumentView view);
		void DisconnectFromDocumentView(DocumentView view);
	}
}
