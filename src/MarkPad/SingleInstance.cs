using System;
using System.Linq;
using System.Windows;
using MarkPad.Shell;

namespace MarkPad
{
    public class EntryPoint
    {
        [STAThread]
        public static void Main(string[] args)
        {
            SingleInstanceManager manager = new SingleInstanceManager();
            manager.Run(args);
        }
    }

    public class SingleInstanceManager : Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase
    {
        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs eventArgs)
        {
            Loader.Start();
            return false;
        }

        protected override void OnStartupNextInstance(Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs eventArgs)
        {
            base.OnStartupNextInstance(eventArgs);
            ((ShellView)Application.Current.MainWindow).StartAgain(eventArgs.CommandLine.ToArray());
        }
    }
}