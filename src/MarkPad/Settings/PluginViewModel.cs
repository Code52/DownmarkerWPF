using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using MarkPad.PluginApi;

namespace MarkPad.Settings
{
	public class PluginViewModel : PropertyChangedBase
	{
		readonly IPlugin _plugin;

		public PluginViewModel(
			IPlugin plugin)
		{
			_plugin = plugin;
		}

		public string Name { get { return _plugin.Name; } }
		public string Version { get { return _plugin.Version; } }
		public string Authors { get { return _plugin.Authors; } }
		public string Description { get { return _plugin.Description; } }

		public bool CanInstall { get { return true; } }
		public void Install()
		{
		}

		public bool CanUninstall { get { return false; } }
		public void Uninstall() { }

		public void Settings()
		{
		}
	}
}
