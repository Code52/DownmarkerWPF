using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using MarkPad.Contracts;
using MarkPad.Document.SpellCheck;
using MarkPad.Plugins;

namespace MarkPad.Infrastructure.Plugins
{
    public class PluginManager : IPluginManager
	{
		public CompositionContainer Container { get; private set; }
		[ImportMany]
		public IEnumerable<IPlugin> Plugins { get; private set; }

		public PluginManager()
		{
			var catalog = 
                new AggregateCatalog(
		            new AssemblyCatalog(typeof (IPlugin).Assembly),
		            new AssemblyCatalog(typeof (IDocumentView).Assembly),
		            new AssemblyCatalog(typeof (PluginSettingsProvider).Assembly),
		            new AssemblyCatalog(typeof (SpellCheckPlugin.SpellCheckPlugin).Assembly));
            
			Container = new CompositionContainer(catalog);
			Container.ComposeParts(this);
		}
	}
}
