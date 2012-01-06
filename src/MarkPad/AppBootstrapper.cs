using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using MarkPad.Framework;
using MarkPad.Services;
using MarkPad.Shell;
using NLog;
using NLog.Config;
using NLog.Targets;
using LogManager = NLog.LogManager;

namespace MarkPad
{
    class AppBootstrapper : Bootstrapper<ShellViewModel>
    {
        private Mutex mutex;

        private IContainer container;
        private JumpListIntegration jumpList;
        private OpenFileListener openFileListenerListener;

        private static void SetupLogging()
        {
            var debuggerTarget = new DebuggerTarget { Layout = "[${level:uppercase=true}] (${logger}) ${message}" };

            var config = new LoggingConfiguration();
            config.AddTarget("debugger", debuggerTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, debuggerTarget));

            LogManager.Configuration = config;
        }

        protected override void Configure()
        {
            SetupLogging();

            Caliburn.Micro.LogManager.GetLog = t => new NLogAdapter(t);

            var builder = new ContainerBuilder();

            SetupCaliburnMicroDefaults(builder);

            builder.RegisterModule<EventAggregationAutoSubscriptionModule>();

            builder.RegisterModule<ServicesModule>();

            builder.RegisterType<JumpListIntegration>().SingleInstance();
            builder.RegisterType<OpenFileAction>().AsSelf();

            container = builder.Build();

            jumpList = container.Resolve<JumpListIntegration>();
        }

        protected override void PrepareApplication()
        {
            Application.Startup += OnStartup;
#if (!DEBUG)
            Application.DispatcherUnhandledException += OnUnhandledException;
#endif
            Application.Exit += OnExit;
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            bool isOwned;
            mutex = new Mutex(true, "Markpad.SingleInstanceCheck", out isOwned);
            if (isOwned)
            {
                openFileListenerListener = new OpenFileListener(container);
                openFileListenerListener.Start();
            }
            else
            {
                mutex = null;
                var client = new OpenFileClient();
                client.SendMessage(e.Args);

                Application.Shutdown();
            }
            
            base.OnStartup(sender, e);
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            jumpList.Dispose();

            if (mutex != null)
            {
                mutex.ReleaseMutex();
            }

            base.OnExit(sender, e);
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
            if (String.IsNullOrWhiteSpace(key))
            {
                if (container.TryResolve(service, out instance))
                    return instance;
            }
            else
            {
                if (container.TryResolveNamed(key, service, out instance))
                    return instance;
            }
            throw new Exception(String.Format("Could not locate any instances of contract {0}.", key ?? service.Name));
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
