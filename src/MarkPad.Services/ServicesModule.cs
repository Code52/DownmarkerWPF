using Autofac;
using MarkPad.Services.Implementation;
using MarkPad.Services.Interfaces;
using MarkPad.Services.Settings;

namespace MarkPad.Services
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SiteContextGenerator>().As<ISiteContextGenerator>();
            builder.RegisterType<DialogService>().As<IDialogService>();
            builder.RegisterType<SettingsProvider>().As<ISettingsProvider>().SingleInstance();
            builder.RegisterType<SpellingService>().As<ISpellingService>().SingleInstance().OnActivating(args =>
            {
                var settingsService = args.Context.Resolve<ISettingsProvider>();

                var language = settingsService.GetSettings<MarkpadSettings>().Language;

                args.Instance.SetLanguage(language);
            });
        }
    }
}
