using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarkPad.PluginApi;

namespace ExamplePlugin
{
	public class ExamplePlugin : ICanCreateNewPage
	{
		public string Name { get { return "Example plugin"; } }
		public string Version { get { return "0.1"; } }
		public string Authors { get { return "Code52"; } }
		public string Description { get { return "An example plugin for MarkPad"; } }

		public string CreateNewPageLabel { get { return "New example plugin page"; } }
		
		public string CreateNewPage()
		{
			return "# Hello from the `Example` extension!";
		}
	}
}
