using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Caliburn.Micro;
using MarkPad.Framework;
using MarkPad.Framework.Events;
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
        private string initialFile;
        private IContainer container;
        private JumpListIntegration jumpList;

        public IContainer Container { get { return container; } }

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

            container = builder.Build();

            jumpList = container.Resolve<JumpListIntegration>();
        }

        protected override void PrepareApplication()
        {
            Application.Startup += OnStartup;

            if (!Debugger.IsAttached)
                Application.DispatcherUnhandledException += OnUnhandledException;

            Application.Exit += OnExit;
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            base.OnStartup(sender, e);

            SetAwesomiumDefaults();

            container.Resolve<IEventAggregator>().Publish(new AppReadyEvent());

            ((App)Application).HandleArguments(Environment.GetCommandLineArgs().Skip(1).ToArray());
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            jumpList.Dispose();

            base.OnExit(sender, e);
        }

        private void SetAwesomiumDefaults()
        {
            var c = new Awesomium.Core.WebCoreConfig
            {
                CustomCSS = @"body { font-family: Segoe UI, sans-serif; font-size:0.8em;}
                              ::-webkit-scrollbar { width: 12px; height: 20px; }
                              ::-webkit-scrollbar-track { background-color: white; }
                              ::-webkit-scrollbar-thumb { background-color: #B9B9B9; }
                              ::-webkit-scrollbar-thumb:hover { background-color: #000000; }",
            };

            Awesomium.Core.WebCore.Initialize(c, true);
            Awesomium.Core.WebCore.BaseDirectory = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                    "Themes"
            );
        }

        private static void SetupCaliburnMicroDefaults(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
              .Where(type => type.Name.EndsWith("ViewModel"))
              .AsSelf()
              .InstancePerDependency();

            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
              .Where(type => type.Name.EndsWith("View"))
              .AsSelf()
              .InstancePerDependency();

            builder.Register<IWindowManager>(c => new WindowManager()).InstancePerLifetimeScope();
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
