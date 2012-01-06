using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Autofac;
using Caliburn.Micro;
using MarkPad.Events;
using Message = System.ServiceModel.Channels.Message;

namespace MarkPad
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

    [ServiceContract]
    public interface IOpenFileAction
    {
        [OperationContract]
        void OpenFile(string path);
    }

    public class OpenFileAction : IOpenFileAction
    {
        private readonly IEventAggregator eventAggregator;

        public OpenFileAction(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public void OpenFile(string path)
        {
            eventAggregator.Publish(new FileOpenEvent(path));
        }
    }

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

    public class AutofacInstanceProviderServiceBehavior : IServiceBehavior
    {
        private readonly IComponentContext context;

        public AutofacInstanceProviderServiceBehavior(IComponentContext context)
        {
            this.context = context;
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            serviceHostBase.ChannelDispatchers.ToList().ForEach(channelDispatcher =>
            {
                var dispatcher = channelDispatcher as ChannelDispatcher;
                if (dispatcher != null)
                {
                    dispatcher.Endpoints.ToList().ForEach(endpoint =>
                    {
                        endpoint.DispatchRuntime.InstanceProvider = new AutofacInstanceProvider(serviceDescription.ServiceType, context);
                    });
                }
            });
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }
}
