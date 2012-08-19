using Caliburn.Micro;
using MarkPad.Plugins;

namespace MarkPad.Settings.UI
{
	public class PluginViewModel : PropertyChangedBase
	{
		readonly IPlugin plugin;

	    public PluginViewModel(IPlugin plugin)
		{
			this.plugin = plugin;
		}

		public string Name { get { return plugin.Name; } }
		public string Version { get { return plugin.Version; } }
		public string Authors { get { return plugin.Authors; } }
		public string Description { get { return plugin.Description; } }
		public bool IsConfigurable { get { return plugin.IsConfigurable; } }

		public bool CanInstall { get { return !plugin.Settings.IsEnabled; } }
		public void Install()
		{
			SetIsEnabled(true);
		}

		public bool CanUninstall { get { return plugin.Settings.IsEnabled; } }
		public void Uninstall()
		{
			SetIsEnabled(false);
		}

		private void SetIsEnabled(bool isEnabled)
		{
			plugin.Settings.IsEnabled = isEnabled;
			plugin.SaveSettings();

			NotifyOfPropertyChange(() => CanInstall);
			NotifyOfPropertyChange(() => CanUninstall);
		}

		public void OpenPluginConfiguration()
		{
		}
	}
}
