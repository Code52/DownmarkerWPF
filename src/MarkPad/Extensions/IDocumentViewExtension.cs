using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarkPad.Document;

namespace MarkPad.Extensions
{
	public interface IDocumentViewExtension : IMarkPadExtension
	{
		void ConnectToDocumentView(DocumentView view);
		void DisconnectFromDocumentView();
	}
}
