using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Autofac;

namespace MarkPad.Services.Services
{
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