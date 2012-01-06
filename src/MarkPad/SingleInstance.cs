using System;
using System.IO;
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

            var args = eventArgs.CommandLine.ToArray();

            if (args.Length == 1)
            {
                var filePath = args[0];
                if (File.Exists(filePath) && Path.GetExtension(filePath) == ".md")
                    ((ShellView)Application.Current.MainWindow).OpenFile(filePath);
            }
        }
    }
}