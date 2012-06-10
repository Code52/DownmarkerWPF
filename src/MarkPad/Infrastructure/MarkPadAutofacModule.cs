using Autofac;
using MarkPad.Contracts;
using MarkPad.Document;
using MarkPad.Document.SpellCheck;
using MarkPad.Infrastructure.Plugins;
using MarkPad.Settings;

namespace MarkPad.Infrastructure
{
	public class MarkPadAutofacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PluginManager>().As<IPluginManager>().SingleInstance();
			builder.RegisterType<DocumentParser>().As<IDocumentParser>();
			builder.RegisterType<SpellingService>().As<ISpellingService>().SingleInstance().OnActivating(args =>
			{
				var settingsService = args.Context.Resolve<ISettingsProvider>();
				var settings = settingsService.GetSettings<SpellCheckPlugin.SpellCheckPluginSettings>();
				args.Instance.SetLanguage(settings.Language);
			});
		}
	}
}
