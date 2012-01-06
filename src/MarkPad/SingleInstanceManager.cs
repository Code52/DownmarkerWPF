using System;
using System.Linq;
using System.Windows;
using Microsoft.VisualBasic.ApplicationServices;
using StartupEventArgs = Microsoft.VisualBasic.ApplicationServices.StartupEventArgs;

namespace MarkPad
{
    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var manager = new SingleInstanceManager();
            manager.Run(args);
        }

        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs eventArgs)
        {
            Loader.Start();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            base.OnStartupNextInstance(eventArgs);
            ((App)Application.Current).HandleArguments(eventArgs.CommandLine.ToArray());
        }
    }
}