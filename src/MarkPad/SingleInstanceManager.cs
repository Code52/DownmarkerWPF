using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MarkPad.Shell;
using Microsoft.VisualBasic.ApplicationServices;
using StartupEventArgs = Microsoft.VisualBasic.ApplicationServices.StartupEventArgs;

namespace MarkPad
{
    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs eventArgs)
        {
            var file = GetFile(eventArgs);

            if (!string.IsNullOrWhiteSpace(file))
            {
                Loader.Start(file);
            }
            else
            {
                Loader.Start();
            }
            
            return false;
        }

        private string GetFile(StartupEventArgs eventArgs)
        {
            var args = eventArgs.CommandLine.ToArray();

            if (args.Length == 1)
            {
                var filePath = args[0];
                if (File.Exists(filePath) && Constants.DefaultExtensions.Contains(Path.GetExtension(filePath).ToLower()))
                    return filePath;
            }

            return string.Empty;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            base.OnStartupNextInstance(eventArgs);

            var args = eventArgs.CommandLine.ToArray();

            if (args.Length != 1) return;

            var filePath = args[0];
            if (File.Exists(filePath) && Path.GetExtension(filePath) == ".md")
                ((ShellView)Application.Current.MainWindow).OpenFile(filePath);
        }
    }
}