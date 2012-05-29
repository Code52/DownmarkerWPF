using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace MarkPad.PluginApi
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
