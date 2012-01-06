using System;
using System.ServiceModel;
using Autofac;
using MarkPad.Services.Services;

namespace MarkPad.Shell
{
    public class OpenFileListener : IDisposable
    {
        private readonly IComponentContext context;
        private ServiceHost host;

        public OpenFileListener(IComponentContext context)
        {
            this.context = context;
        }

        public void Start()
        {
            var factory = new AutofacServiceFactory(context);
            host = factory.CreateService(typeof(OpenFileAction), new[] { new Uri("net.pipe://localhost") });
            host.AddServiceEndpoint(typeof(IOpenFileAction), new NetNamedPipeBinding(), "OpenFileAction");
            host.Open();
        }

        public void Dispose()
        {
            host.Close();
        }
    }
}
