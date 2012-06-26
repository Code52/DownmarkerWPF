using Caliburn.Micro;
using MarkPad.Events;
using MarkPad.Plugins;

namespace MarkPad.Settings.UI
{
	public class PluginViewModel : PropertyChangedBase
	{
		readonly IPlugin _plugin;
		readonly IEventAggregator _eventAggregator;

		public PluginViewModel(
			IPlugin plugin,
			IEventAggregator eventAggregator)
		{
			_plugin = plugin;
			_eventAggregator = eventAggregator;
		}

		public string Name { get { return _plugin.Name; } }
		public string Version { get { return _plugin.Version; } }
		public string Authors { get { return _plugin.Authors; } }
		public string Description { get { return _plugin.Description; } }
		public bool IsConfigurable { get { return _plugin.IsConfigurable; } }

		public bool CanInstall { get { return !_plugin.Settings.IsEnabled; } }
		public void Install()
		{
			SetIsEnabled(true);
		}

		public bool CanUninstall { get { return _plugin.Settings.IsEnabled; } }
		public void Uninstall()
		{
			SetIsEnabled(false);
		}

		private void SetIsEnabled(bool isEnabled)
		{
			_plugin.Settings.IsEnabled = isEnabled;
			_plugin.SaveSettings();

			this.NotifyOfPropertyChange(() => CanInstall);
			this.NotifyOfPropertyChange(() => CanUninstall);
			_eventAggregator.Publish(new PluginsChangedEvent());
		}

		public void OpenPluginConfiguration()
		{
		}
	}
}
