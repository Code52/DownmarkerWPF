using Autofac;
using MarkPad.Services.Implementation;
using MarkPad.Services.Interfaces;
using MarkPad.Services.Metaweblog.Rsd;
using MarkPad.Services.Settings;

namespace MarkPad.Services
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RsdService>().As<IRsdService>();
            builder.RegisterType<SiteContextGenerator>().As<ISiteContextGenerator>();
            builder.RegisterType<DialogService>().As<IDialogService>();
            builder.RegisterType<MetaWeblogService>().As<IMetaWeblogService>();
            builder.RegisterType<TaskSchedulerFactory>().As<ITaskSchedulerFactory>();
            builder.RegisterType<SettingsProvider>().As<ISettingsProvider>().SingleInstance();
            builder.RegisterType<SpellingService>().As<ISpellingService>().SingleInstance().OnActivating(args =>
            {
                var settingsService = args.Context.Resolve<ISettingsProvider>();
                var settings = settingsService.GetSettings<MarkPadSettings>();
                args.Instance.SetLanguage(settings.Language);    
            });
        }
    }
}
