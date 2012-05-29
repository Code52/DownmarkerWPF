using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using MarkPad.Contracts;

namespace MarkPad.PluginApi
{
	[InheritedExport]
	public interface ICanSavePage : IPlugin
	{
		string SavePageLabel { get; }
		void SavePage(IDocumentViewModel documentViewModel);
	}
}
