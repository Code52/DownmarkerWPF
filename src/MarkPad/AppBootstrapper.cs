using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Caliburn.Micro;
using MarkPad.Framework;
using MarkPad.Shell;
using Microsoft.Win32;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace MarkPad
{
    class AppBootstrapper : Bootstrapper<ShellViewModel>
    {
        private IContainer container;

        private void SetupLogging()
        {
            var debuggerTarget = new DebuggerTarget { Layout = "[${level:uppercase=true}] (${logger}) ${message}" };

            var config = new LoggingConfiguration();
            config.AddTarget("debugger", debuggerTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, debuggerTarget));

            NLog.LogManager.Configuration = config;
        }

        protected override void Configure()
        {
            SetupLogging();

            Caliburn.Micro.LogManager.GetLog = t => new NLogAdapter(t);

            var builder = new ContainerBuilder();

            SetupCaliburnMicroDefaults(builder);

            builder.RegisterModule<EventAggregationAutoSubscriptionModule>();

            builder.RegisterModule<Services.ServicesModule>();

            container = builder.Build();

            SetAwesomiumDefaults();

            RegisterExtensionWithApp();
        }

        protected override void PrepareApplication()
        {
            Application.Startup += OnStartup;
#if (!DEBUG)
            Application.DispatcherUnhandledException += OnUnhandledException;
#endif
            Application.Exit += OnExit;
        }

        private void SetAwesomiumDefaults()
        {
            var c = new Awesomium.Core.WebCoreConfig
            {
                CustomCSS = @"::-webkit-scrollbar { width: 12px; height: 20px; }
                              ::-webkit-scrollbar-track { background-color: white; }
                              ::-webkit-scrollbar-thumb { background-color: #B9B9B9; }
                              ::-webkit-scrollbar-thumb:hover { background-color: #000000; }",
            };

            Awesomium.Core.WebCore.Initialize(c);
        }

        private void RegisterExtensionWithApp()
        {
            string markpadKeyName = "markpad.md";
            string defaultExtension = ".md";

            string exePath = Assembly.GetEntryAssembly().Location;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes", true))
            {
                using (RegistryKey extensionKey = key.CreateSubKey(defaultExtension))
                {
                    extensionKey.SetValue("", markpadKeyName);
                }

                using (RegistryKey markpadKey = key.CreateSubKey(markpadKeyName))
                {
                    using (RegistryKey defaultIconKey = markpadKey.CreateSubKey("DefaultIcon"))
                    {
                        defaultIconKey.SetValue("", exePath + ",0");
                    }

                    using (RegistryKey shellKey = markpadKey.CreateSubKey("shell"))
                    {
                        using (RegistryKey openKey = shellKey.CreateSubKey("open"))
                        {
                            using (RegistryKey commandKey = openKey.CreateSubKey("command"))
                            {
                                commandKey.SetValue("", "\"" + exePath + "\" \"%1\"");
                            }
                        }
                    }
                }
            }
        }

        private static void SetupCaliburnMicroDefaults(ContainerBuilder builder)
        {
            //  register view models
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                //  must be a type with a name that ends with ViewModel
              .Where(type => type.Name.EndsWith("ViewModel"))
                //  registered as self
              .AsSelf()
                //  always create a new one
              .InstancePerDependency();

            //  register views
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                //  must be a type with a name that ends with View
              .Where(type => type.Name.EndsWith("View"))
                //  registered as self
              .AsSelf()
                //  always create a new one
              .InstancePerDependency();

            //  register the single window manager for this container
            builder.Register<IWindowManager>(c => new WindowManager()).InstancePerLifetimeScope();
            //  register the single event aggregator for this container
            builder.Register<IEventAggregator>(c => new EventAggregator()).InstancePerLifetimeScope();
        }

        protected override object GetInstance(Type service, string key)
        {
            object instance;
            if (string.IsNullOrWhiteSpace(key))
            {
                if (container.TryResolve(service, out instance))
                    return instance;
            }
            else
            {
                if (container.TryResolveNamed(key, service, out instance))
                    return instance;
            }
            throw new Exception(string.Format("Could not locate any instances of contract {0}.", key ?? service.Name));
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            container.InjectProperties(instance);
        }
    }
}
