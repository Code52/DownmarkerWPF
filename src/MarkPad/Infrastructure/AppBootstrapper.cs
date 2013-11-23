using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using Caliburn.Micro;
using Caliburn.Micro.Autofac;
using MarkPad.Events;
using MarkPad.Infrastructure.CaliburnExtensions;
using MarkPad.Preview;

namespace MarkPad.Infrastructure
{
    class AppBootstrapper : AutofacBootstrapper<ShellViewModel>
    {
        JumpListIntegration jumpList;

        static AppBootstrapper()
        {
            LogManager.GetLog = t => new DebugLogger(t);
        }

        protected override void ConfigureBootstrapper()
        {
            //  you must call the base version first!
            base.ConfigureBootstrapper();
            //  override namespace naming convention
            EnforceNamespaceConvention = false;
            //  auto subsubscribe event aggregators
            AutoSubscribeEventAggegatorHandlers = true;

            FrameworkExtensions.Message.Attach.AllowXamlSyntax();
        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<ServicesModule>();
            builder.RegisterType<JumpListIntegration>().SingleInstance();
			builder.RegisterModule<MarkPadAutofacModule>();
            builder.RegisterType<ShellViewModel>().AsSelf().As<IShell>().SingleInstance();
        }

        protected override void PrepareApplication()
        {
            Application.Startup += OnStartup;

            if (!Debugger.IsAttached)
                Application.DispatcherUnhandledException += OnUnhandledException;

            Application.Exit += OnExit;
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);

            jumpList = Container.Resolve<JumpListIntegration>();

            SetAwesomiumDefaults();

            DumpIconsForDocuments();

            ExtendCaliburn();

            Container.Resolve<IEventAggregator>().Publish(new AppReadyEvent());

            // Handle the original arguments from the first run of this app.
            var activationArguments = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;
            if (activationArguments == null || activationArguments.ActivationData == null)
                ((App)Application).HandleArguments(Environment.GetCommandLineArgs());
            else
            {
                ((App)Application).HandleArguments(activationArguments.ActivationData);
            }
        }
                
        protected override void OnExit(object sender, EventArgs e)
        {
            jumpList.Dispose();

            base.OnExit(sender, e);
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                ShowDialog(e.Exception);
            }
            catch
            {
                // We don't care about an exception in the exception handler, so swallow it.
            }

            e.Handled = true;
        }

        private void SetAwesomiumDefaults()
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                ?? Directory.GetCurrentDirectory();

            HtmlPreview.BaseDirectory = Path.Combine(directoryName, "Themes");
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

                var executingAssembly = Assembly.GetExecutingAssembly();
                using (Stream stm = executingAssembly.GetManifestResourceStream(String.Format("{0}.{1}", assemblyName.Name, file)))
                using (Stream outFile = File.Create(Path.Combine(Constants.IconDir, file)))
                {
                    if (stm != null) 
                        stm.CopyTo(outFile);
                }
            }
        }

        private void ExtendCaliburn()
        {
            MessageBinder.SpecialValues.Add("$filenames", context =>
            {
                var args = context.EventArgs as DragEventArgs;

                if (args == null || !args.Data.GetDataPresent(DataFormats.FileDrop)) 
                    return null;
                
                return (string[])args.Data.GetData(DataFormats.FileDrop);
            });
        }

        public IEventAggregator GetEventAggregator()
        {
            return Container.Resolve<IEventAggregator>();
        }

        private static void ShowDialog(Exception e)
        {
            Exception inner = e;
            while (inner.InnerException != null)
            {
                inner = inner.InnerException;
            }

            var dialog = new ExceptionDialog
            {
                Message = inner.Message, 
                Details = ExceptionBuilder.ExceptionToString(e), 
                Exception = e
            };
            dialog.ShowDialog();
        }
    }
}
