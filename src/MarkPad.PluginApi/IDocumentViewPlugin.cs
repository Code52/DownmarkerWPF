using MarkPad.Contracts;
using System.ComponentModel.Composition;

namespace MarkPad.Plugins
{
	[InheritedExport]
	public interface IDocumentViewPlugin : IPlugin
	{
		void ConnectToDocumentView(IDocumentView view);
		void DisconnectFromDocumentView(IDocumentView view);
	}
}
