using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
