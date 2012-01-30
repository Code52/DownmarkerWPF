using Autofac;
using MarkPad.Services.Implementation;
using MarkPad.Services.Interfaces;

namespace MarkPad.Services
{
    public class ServicesModule : Module
    {
        private const string DictionariesSettingsKey = "Dictionaries";

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DialogService>().As<IDialogService>();
            builder.RegisterType<SettingsService>().As<ISettingsService>().SingleInstance();
            builder.RegisterType<SpellingService>().As<ISpellingService>().SingleInstance().OnActivating(args =>
            {
                var settingsService = args.Context.Resolve<ISettingsService>();

                var language = settingsService.Get<SpellingLanguages>(DictionariesSettingsKey);

                args.Instance.SetLanguage(language);
            });
        }
    }
}
