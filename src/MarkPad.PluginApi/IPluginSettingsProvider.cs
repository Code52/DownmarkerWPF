using System.ComponentModel.Composition;

namespace MarkPad.Plugins
{
	[InheritedExport]
	public interface IPluginSettingsProvider
	{
		T GetSettings<T>() where T : IPluginSettings, new();
		void SaveSettings<T>(T settings) where T : IPluginSettings, new();
	}
}
