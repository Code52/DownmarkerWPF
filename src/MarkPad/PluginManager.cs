using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using MarkPad.PluginApi;

namespace MarkPad
{
	public interface IPluginManager
	{
		CompositionContainer Container { get; }
		IEnumerable<IPlugin> Plugins { get; }
	}

	public class PluginManager : IPluginManager
	{
		public CompositionContainer Container { get; private set; }
		[ImportMany]
		public IEnumerable<IPlugin> Plugins { get; private set; }

		public PluginManager()
		{
			var catalog = new AggregateCatalog();
			
			catalog.Catalogs.Add(new AssemblyCatalog(typeof(ExamplePlugin.ExamplePlugin).Assembly));

			Container = new CompositionContainer(catalog);
			Container.ComposeParts(this);
		}
	}
}
