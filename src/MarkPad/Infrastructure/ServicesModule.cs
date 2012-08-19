using Autofac;
using MarkPad.DocumentSources;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.DocumentSources.MetaWeblog.Service.Rsd;
using MarkPad.Infrastructure.Abstractions;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Settings;

namespace MarkPad.Infrastructure
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WebRequestFactory>().As<IWebRequestFactory>();
            builder.RegisterType<RsdService>().As<IRsdService>();
            builder.RegisterType<SiteContextGenerator>().As<ISiteContextGenerator>();
            builder.RegisterType<DialogService.DialogService>().As<IDialogService>();
            builder.RegisterType<MetaWeblogService>().As<IMetaWeblogService>();
            builder.RegisterType<TaskSchedulerFactory>().As<ITaskSchedulerFactory>();
            builder.RegisterType<SettingsProvider>().As<ISettingsProvider>().SingleInstance();
        }
    }
}
