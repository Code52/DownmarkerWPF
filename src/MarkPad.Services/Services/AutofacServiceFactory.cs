using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Autofac;

namespace MarkPad.Services.Services
{
    public class AutofacServiceFactory : ServiceHostFactory
    {
        private readonly IComponentContext context;

        public AutofacServiceFactory(IComponentContext context)
        {
            this.context = context;
        }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new AutofacServiceHost(context, serviceType, baseAddresses);
        }

        public ServiceHost CreateService(Type serviceType, Uri[] baseAddresses)
        {
            return CreateServiceHost(serviceType, baseAddresses);
        }
    }
}