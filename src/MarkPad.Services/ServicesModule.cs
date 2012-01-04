using Autofac;
using MarkPad.Services.Implementation;
using MarkPad.Services.Interfaces;

namespace MarkPad.Services
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DialogService>().As<IDialogService>();
            builder.RegisterType<SettingsService>().As<ISettingsService>();
        }
    }
}
