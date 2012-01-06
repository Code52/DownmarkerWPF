using System;
using System.ServiceModel;
using Autofac;

namespace MarkPad.Services.Services
{
    public class AutofacServiceHost : ServiceHost
    {
        private readonly IComponentContext context;

        public AutofacServiceHost(IComponentContext context)
        {
            this.context = context;
        }

        public AutofacServiceHost(IComponentContext context, Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            this.context = context;
        }

        protected override void OnOpening()
        {
            Description.Behaviors.Add(new AutofacInstanceProviderServiceBehavior(context));
            base.OnOpening();
        }
    }
}