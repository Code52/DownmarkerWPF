using Autofac;
using MarkPad.Document;
using MarkPad.Document.EditorBehaviours;
using MarkPad.Document.Search;
using MarkPad.Document.SpellCheck;
using MarkPad.DocumentSources;
using MarkPad.DocumentSources.MetaWeblog;
using MarkPad.Infrastructure.Abstractions;
using MarkPad.Infrastructure.Plugins;
using MarkPad.Plugins;
using MarkPad.Settings;
using MarkPad.Settings.Models;

namespace MarkPad.Infrastructure
{
	public class MarkPadAutofacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
		    foreach (var plugin in new PluginManager().Plugins)
		    {
		        builder.RegisterInstance(plugin).AsImplementedInterfaces();
		    }

		    builder.RegisterType<BlogService>().As<IBlogService>();
		    builder.RegisterType<DocumentFactory>().As<IDocumentFactory>();
		    builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();
		    builder.RegisterType<FileSystemWatcherFactory>().As<IFileSystemWatcherFactory>().SingleInstance();
			builder.RegisterType<DocumentParser>().As<IDocumentParser>();
		    builder.RegisterType<SpellCheckProvider>().As<ISpellCheckProvider>();
			builder.RegisterType<SpellingService>().As<ISpellingService>().SingleInstance().OnActivating(args =>
			{
				var settingsService = args.Context.Resolve<ISettingsProvider>();
				var settings = settingsService.GetSettings<MarkPadSettings>();
				args.Instance.SetLanguage(settings.Language);
			});
            builder.RegisterType<SearchProvider>().As<ISearchProvider>();
            builder.RegisterType<SearchSettings>().SingleInstance();
            builder.RegisterType<PairedCharsHighlightProvider>().As<IPairedCharsHighlightProvider>();
		}
	}
}
