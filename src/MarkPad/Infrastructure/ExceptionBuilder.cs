using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MarkPad.Infrastructure
{
    public class ExceptionBuilder
    {
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

        public static string ExceptionToString(Exception exception)
        {
            return ExceptionToString(exception, true);
        }

        private static string ExceptionToString(Exception exception, bool includeSysInfo)
        {
            var exceptionString = new StringBuilder();

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
            exceptionString.AppendLine(ExceptionPartToString("", exception, EnhancedStackTrace));

            return exceptionString.ToString();
        }

        private static string EnhancedStackTrace(Exception exception)
        {
            return EnhancedStackTrace(new StackTrace(exception, true));
        }

        private static string EnhancedStackTrace(StackTrace stackTrace)
        {
            var sb = new StringBuilder();

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
            var sb = new StringBuilder();
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
            var sysinfo = new StringBuilder();

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
            get { return parentAssembly ?? (parentAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()); }
        } 
    }
}