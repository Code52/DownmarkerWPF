using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using MarkPad.PluginApi;

namespace MarkPad.Services.Settings
{
	/// <summary>
	/// This is a wrapper for SettingsProvider which can be injected into a plugin using MEF
	/// </summary>
	public class PluginSettingsProvider : IPluginSettingsProvider
	{
		readonly ISettingsProvider _settingsProvider = new SettingsProvider();

		public T GetSettings<T>() where T : IPluginSettings, new()
		{
			return _settingsProvider.GetSettings<T>();
		}

		public void SaveSettings<T>(T settings) where T : IPluginSettings, new()
		{
			_settingsProvider.SaveSettings(settings);
		}
	}
}
