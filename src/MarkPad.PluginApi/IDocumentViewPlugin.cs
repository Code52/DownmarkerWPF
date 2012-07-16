using System.ComponentModel.Composition;
using MarkPad.Contracts;

namespace MarkPad.Plugins
{
    [InheritedExport]
	public interface IDocumentViewPlugin : IPlugin
	{
		void ConnectToDocumentView(IDocumentView view);
		void DisconnectFromDocumentView(IDocumentView view);
	}
}
