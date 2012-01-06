using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Autofac;

namespace MarkPad.Services.Services
{
    public class AutofacInstanceProvider : IInstanceProvider
    {
        private readonly Type serviceType;
        private readonly IComponentContext context;

        public AutofacInstanceProvider(Type serviceType, IComponentContext context)
        {
            this.serviceType = serviceType;
            this.context = context;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return context.Resolve(serviceType);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            if (instance is IDisposable)
                ((IDisposable)instance).Dispose();
        }
    }
}