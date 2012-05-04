using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarkPad.PluginApi;
using System.ComponentModel.Composition;
using MarkPad.Contracts;
using System.ComponentModel;

namespace ExamplePlugin
{
	public class ExamplePlugin : ICanCreateNewPage
	{
		readonly IPluginSettingsProvider _settingsProvider;

		private ExampleSettings _settings;

		public string Name { get { return "Example plugin"; } }
		public string Version { get { return "0.1"; } }
		public string Authors { get { return "Code52"; } }
		public string Description { get { return "An example plugin for MarkPad"; } }
		public IPluginSettings Settings { get { return _settings; } }
		public bool IsConfigurable { get { return false; } }
		public bool IsHidden { get { return true; } }

		public string CreateNewPageLabel { get { return "New example plugin page"; } }

		[ImportingConstructor]
		public ExamplePlugin(IPluginSettingsProvider settingsProvider)
		{
			_settingsProvider = settingsProvider;

			_settings = _settingsProvider.GetSettings<ExampleSettings>();
		}

		public void SaveSettings()
		{
			_settingsProvider.SaveSettings(_settings);
		}
		
		public string CreateNewPage()
		{
			return "# Hello from the `Example` extension!";
		}
	}

	public class ExampleSettings : IPluginSettings
	{
		[DefaultValue(false)]
		public bool IsEnabled { get; set; }
	}
}
