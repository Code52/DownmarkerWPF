using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarkPad.PluginApi;

namespace ExamplePlugin
{
	public class ExamplePlugin : ICanCreateNewPage
	{
		public string CreateNewPageLabel { get { return "New example plugin page"; } }
		
		public string CreateNewPage()
		{
			return "# Hello from the `Example` extension!";
		}
	}
}
