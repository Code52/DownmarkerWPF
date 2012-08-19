using System.ComponentModel.Composition;

namespace MarkPad.Plugins
{
	[InheritedExport]
	public interface IPlugin
	{
		string Name { get; }
		string Version { get; }
		string Authors { get; }
		string Description { get; }
		IPluginSettings Settings { get; }
		void SaveSettings();
		bool IsConfigurable { get; }
		bool IsHidden { get; }
	}
}
