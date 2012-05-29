using System.ComponentModel;

namespace MarkPad.Plugins
{
	public interface IPluginSettings
	{
		bool IsEnabled { get; set; }
	}

	public class PluginSettings : IPluginSettings
	{
		[DefaultValue(true)]
		public bool IsEnabled { get; set; }
	}
}
