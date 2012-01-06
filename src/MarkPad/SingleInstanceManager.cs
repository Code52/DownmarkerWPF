using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MarkPad.Shell;
using StartupEventArgs = Microsoft.VisualBasic.ApplicationServices.StartupEventArgs;

namespace MarkPad
{
    public class SingleInstanceManager : Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase
    {
        private IList<string> validExtensions = new[] { ".md", ".markdown", ".mdown" };

        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs eventArgs)
        {
            var file = GetFile(eventArgs);

            if (!string.IsNullOrWhiteSpace(file))
            {
                Loader.Start(file);

                // Loader.Start with filename
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
                if (File.Exists(filePath) && validExtensions.Contains(Path.GetExtension(filePath)))
                    return filePath;
            }

            return string.Empty;
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