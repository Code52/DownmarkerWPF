using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Threading;
using Autofac;
using Caliburn.Micro;
using MarkPad.Framework;
using MarkPad.Framework.Events;
using MarkPad.Services;
using MarkPad.Shell;
using System.Windows;

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

            FrameworkExtensions.Message.Attach.AllowXamlSyntax();
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

            ExtendCaliburn();

            Container.Resolve<IEventAggregator>().Publish(new AppReadyEvent());

            // Handle the original arguments from the first run of this app.
            ((App)Application).HandleArguments(Environment.GetCommandLineArgs());
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

        private void ExtendCaliburn()
        {
            MessageBinder.SpecialValues.Add("$filenames", context =>
            {
                var args = context.EventArgs as DragEventArgs;

                if (args == null || !args.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)) 
                    return null;
                
                return (string[])args.Data.GetData(System.Windows.DataFormats.FileDrop);
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

            ExceptionDialog dialog = new ExceptionDialog();
            dialog.Message = inner.Message;
            dialog.Details = ExceptionToString(e);
            dialog.Exception = e;
            dialog.ShowDialog();
        }

        #region Error message producers

        private static string ExceptionPartToString(string header, Exception exception, Func<Exception, string> map)
        {
            string part;

            try
            {
                part = map(exception);
            }
            catch (Exception e)
            {
                part = e.Message;
            }

            return header + part;
        }

        private static string FunctionToString(string header, Func<string> func)
        {
            string part;

            try
            {
                part = func();
            }
            catch (Exception e)
            {
                part = e.Message;
            }

            return header + part;
        }

        private static string ExceptionToString(Exception exception)
        {
            return ExceptionToString(exception, true);
        }

        private static string ExceptionToString(Exception exception, bool includeSysInfo)
        {
            StringBuilder exceptionString = new StringBuilder();

            if (exception.InnerException != null)
            {
                exceptionString.AppendLine("(Inner Exception)");
                exceptionString.AppendLine(ExceptionToString(exception.InnerException, false));
                exceptionString.AppendLine("(Outer Exception)");
            }

            if (includeSysInfo)
                exceptionString.Append(SysInfoToString());

            exceptionString.AppendLine(ExceptionPartToString("Exception Source:      ", exception, e => e.Source));
            exceptionString.AppendLine(ExceptionPartToString("Exception Type:        ", exception, e => e.GetType().FullName));
            exceptionString.AppendLine(ExceptionPartToString("Exception Message:     ", exception, e => e.Message));
            exceptionString.AppendLine(ExceptionPartToString("Exception Target Site: ", exception, e => e.TargetSite.Name));
            exceptionString.AppendLine(ExceptionPartToString("", exception, e => EnhancedStackTrace(e)));

            return exceptionString.ToString();
        }

        private static string EnhancedStackTrace(Exception exception)
        {
            return EnhancedStackTrace(new StackTrace(exception, true));
        }

        private static string EnhancedStackTrace(StackTrace stackTrace)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine();
            sb.Append("---- Stack Trace ----");
            sb.AppendLine();

            for (int frame = 0; frame < stackTrace.FrameCount; frame++)
            {
                StackFrame sf = stackTrace.GetFrame(frame);

                sb.Append(StackFrameToString(sf));
            }
            sb.AppendLine();

            return sb.ToString();
        }

        private static string StackFrameToString(StackFrame sf)
        {
            StringBuilder sb = new StringBuilder();
            MemberInfo mi = sf.GetMethod();

            sb.AppendFormat("   {0}.{1}.{2}",
                mi.DeclaringType.Namespace,
                mi.DeclaringType.Name,
                mi.Name);

            ParameterInfo[] parameters = sf.GetMethod().GetParameters();
            sb.Append("(");
            sb.Append(String.Join(", ", parameters.Select(p => String.Format("{0} {1}", p.ParameterType.Name, p.Name)).ToArray()));
            sb.Append(")");
            sb.AppendLine();

            sb.Append("       ");
            if (String.IsNullOrEmpty(sf.GetFileName()))
            {
                sb.Append(Path.GetFileName(ParentAssembly.CodeBase));
                sb.Append(": N ");
                sb.AppendFormat("{0:#00000}", sf.GetNativeOffset());
            }
            else
            {
                sb.Append(Path.GetFileName(sf.GetFileName()));
                sb.AppendFormat(": line {0:#0000}, col {1:#00}", sf.GetFileLineNumber(), sf.GetFileColumnNumber());
                if (sf.GetILOffset() != StackFrame.OFFSET_UNKNOWN)
                {
                    sb.AppendFormat(", IL {0:#0000}", sf.GetILOffset());
                }
            }
            sb.AppendLine();

            return sb.ToString();
        }

        private static string SysInfoToString()
        {
            StringBuilder sysinfo = new StringBuilder();

            sysinfo.AppendFormat("Date and Time:         {0}{1}", DateTime.Now, Environment.NewLine);
            sysinfo.AppendLine(FunctionToString("OS Version:            ", () => Environment.OSVersion.VersionString));
            sysinfo.AppendLine();

            sysinfo.AppendLine(FunctionToString("Application Domain:    ", () => AppDomain.CurrentDomain.FriendlyName));
            sysinfo.AppendLine(FunctionToString("Assembly Codebase:     ", () => ParentAssembly.CodeBase));
            sysinfo.AppendLine(FunctionToString("Assembly Full Name:    ", () => ParentAssembly.FullName));
            sysinfo.AppendLine(FunctionToString("Assembly Version:      ", () => ParentAssembly.GetName().Version.ToString()));
            sysinfo.AppendLine(FunctionToString("Assembly Build Date:   ", () => AssemblyBuildDate(ParentAssembly).ToString()));
            sysinfo.AppendLine();

            return sysinfo.ToString();
        }

        private static DateTime AssemblyBuildDate(Assembly parentAssembly)
        {
            try
            {
                Version v = parentAssembly.GetName().Version;

                DateTime buildDate = new DateTime(2000, 1, 1).AddDays(v.Build).AddSeconds(v.Revision * 2);

                if (TimeZone.IsDaylightSavingTime(DateTime.Now, TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year)))
                    buildDate = buildDate.AddHours(1);

                if (buildDate > DateTime.Now || v.Build < 730 || v.Revision == 0)
                    buildDate = AssemblyFileTime(parentAssembly);

                return buildDate;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private static DateTime AssemblyFileTime(Assembly parentAssembly)
        {
            try
            {
                return File.GetLastWriteTime(parentAssembly.Location);
            }
            catch (Exception)
            {
                return DateTime.MaxValue;
            }
        }

        private static Assembly parentAssembly;
        private static Assembly ParentAssembly
        {
            get
            {
                if (parentAssembly == null)
                {
                    if (Assembly.GetEntryAssembly() == null)
                        parentAssembly = Assembly.GetCallingAssembly();
                    else
                        parentAssembly = Assembly.GetEntryAssembly();
                }

                return parentAssembly;
            }
        }

        #endregion
    }
}
