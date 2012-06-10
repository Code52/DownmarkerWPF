using Caliburn.Micro;
using MarkPad.Framework.Events;
using MarkPad.Plugins;

namespace MarkPad.Settings
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
        public bool IsInstalled
        {
            get
            {
                return _plugin.Settings.IsEnabled;
            }
            set
            {
                if (_plugin.Settings.IsEnabled == value) return;

                _plugin.Settings.IsEnabled = value;
                _plugin.SaveSettings();

                this.NotifyOfPropertyChange(() => IsInstalled);
                _eventAggregator.Publish(new PluginsChangedEvent());
            }
        }

		public void OpenPluginConfiguration()
		{
		}
	}
}
