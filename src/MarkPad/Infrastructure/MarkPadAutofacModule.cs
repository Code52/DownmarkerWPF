using System.IO.Abstractions;
using Autofac;
using MarkPad.Document;
using MarkPad.Document.SpellCheck;
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
		}
	}
}
