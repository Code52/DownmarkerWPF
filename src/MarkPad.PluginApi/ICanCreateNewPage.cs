using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace MarkPad.PluginApi
{
	[InheritedExport]
	public interface ICanCreateNewPage : IPlugin
	{
		string CreateNewPageLabel { get; }
		string CreateNewPage();
	}
}
