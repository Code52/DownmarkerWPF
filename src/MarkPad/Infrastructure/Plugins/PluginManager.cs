using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using MarkPad.Contracts;
using MarkPad.Plugins;
using MarkPad.Services.Settings;

namespace MarkPad.Infrastructure.Plugins
{
    public class PluginManager : IPluginManager
	{
		public CompositionContainer Container { get; private set; }
		[ImportMany]
		public IEnumerable<IPlugin> Plugins { get; private set; }

		public PluginManager()
		{
			var catalog = new AggregateCatalog();

			catalog.Catalogs.Add(new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly()));
			catalog.Catalogs.Add(new AssemblyCatalog(typeof(IPlugin).Assembly));
			catalog.Catalogs.Add(new AssemblyCatalog(typeof(IDocumentView).Assembly));
			catalog.Catalogs.Add(new AssemblyCatalog(typeof(PluginSettingsProvider).Assembly));

            //catalog.Catalogs.Add(new AssemblyCatalog(typeof(ExamplePlugin.ExamplePlugin).Assembly));
            //catalog.Catalogs.Add(new AssemblyCatalog(typeof(ExportToHtmlPlugin.ExportToHtmlPlugin).Assembly));
            //catalog.Catalogs.Add(new AssemblyCatalog(typeof(SpellCheckPlugin.SpellCheckPlugin).Assembly));

			Container = new CompositionContainer(catalog);
			Container.ComposeParts(this);
		}
	}
}
