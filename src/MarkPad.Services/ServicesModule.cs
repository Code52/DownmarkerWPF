using System;
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

                var settings = settingsService.GetSettings<BlogSetting>();

                SpellingLanguages language;
                if (Enum.TryParse(settings.Language, out language))
                {
                    args.Instance.SetLanguage(language);    
                }
            });
        }
    }
}
