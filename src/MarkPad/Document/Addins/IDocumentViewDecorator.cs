using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkPad.Document.Addins
{
	public interface IDocumentViewAddin
	{
		void ConnectTo(DocumentView view);
		void Disconnect();
	}
}
