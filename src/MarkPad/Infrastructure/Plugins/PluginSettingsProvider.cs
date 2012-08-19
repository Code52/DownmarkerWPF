using MarkPad.Plugins;
using MarkPad.Settings;

namespace MarkPad.Infrastructure.Plugins
{
	/// <summary>
	/// This is a wrapper for SettingsProvider which can be injected into a plugin using MEF
	/// </summary>
	public class PluginSettingsProvider : IPluginSettingsProvider
	{
		readonly ISettingsProvider settingsProvider = new SettingsProvider();

		public T GetSettings<T>() where T : IPluginSettings, new()
		{
			return settingsProvider.GetSettings<T>();
		}

		public void SaveSettings<T>(T settings) where T : IPluginSettings, new()
		{
			settingsProvider.SaveSettings(settings);
		}
	}
}
