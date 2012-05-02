using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace MarkPad.PluginApi
{
	[InheritedExport]
	public interface IPlugin
	{
		string Name { get; }
		string Version { get; }
		string Authors { get; }
		string Description { get; }
	}
}
