using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarkPad.Contracts;
using System.ComponentModel.Composition;

namespace MarkPad.PluginApi
{
	[InheritedExport]
	public interface IDocumentViewPlugin : IPlugin
	{
		void ConnectToDocumentView(IDocumentView view);
		void DisconnectFromDocumentView(IDocumentView view);
	}
}
