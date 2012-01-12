using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autofac;
using Caliburn.Micro;
using MarkPad.Framework;
using MarkPad.Framework.Events;
using MarkPad.Services;
using MarkPad.Shell;

namespace MarkPad
{
    class AppBootstrapper : Caliburn.Micro.Autofac.AutofacBootstrapper<ShellViewModel>
    {
        private JumpListIntegration jumpList;

        static AppBootstrapper()
        {
            LogManager.GetLog = t => new DebugLogger(t);
        }

        protected override void ConfigureBootstrapper()
        {   //  you must call the base version first!
            base.ConfigureBootstrapper();
            //  override namespace naming convention
            EnforceNamespaceConvention = false;
            //  auto subsubscribe event aggregators
            AutoSubscribeEventAggegatorHandlers = true;
        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<ServicesModule>();
            builder.RegisterType<JumpListIntegration>().SingleInstance();
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

            jumpList = Container.Resolve<JumpListIntegration>();

            SetAwesomiumDefaults();

            DumpIconsForDocuments();

            Container.Resolve<IEventAggregator>().Publish(new AppReadyEvent());

            // Handle the original arguments from the first run of this app.
            ((App)Application).HandleArguments(Environment.GetCommandLineArgs());
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

        private void DumpIconsForDocuments()
        {
            var assemblyName = Assembly.GetEntryAssembly().GetName();

            if (!Directory.Exists(Constants.IconDir))
                Directory.CreateDirectory(Constants.IconDir);

            foreach (var file in Constants.Icons)
            {
                if (File.Exists(Path.Combine(Constants.IconDir, file)))
                    continue;

                using (Stream stm = Assembly.GetExecutingAssembly().GetManifestResourceStream(String.Format("{0}.{1}", assemblyName.Name, file)))
                using (Stream outFile = File.Create(Path.Combine(Constants.IconDir, file)))
                {
                    stm.CopyTo(outFile);
                }
            }
        }

        public IEventAggregator GetEventAggregator()
        {
            return Container.Resolve<IEventAggregator>();
        }
    }
}
