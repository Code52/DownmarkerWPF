using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace MarkPad.PluginApi
{
	[InheritedExport]
	public interface IPluginSettingsProvider
	{
		T GetSettings<T>() where T : IPluginSettings, new();
		void SaveSettings<T>(T settings) where T : IPluginSettings, new();
	}
}
