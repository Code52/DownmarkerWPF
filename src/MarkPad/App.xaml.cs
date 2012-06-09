using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MarkPad.Events;
using MarkPad.Infrastructure;

namespace MarkPad
{
    public partial class App : ISingleInstanceApp
    {
        private const string Unique = "There can be only one MARKPAD!!! (We ignore crappy sequels here)";

        private readonly AppBootstrapper bootstrapper;

        public App()
        {
            InitializeComponent();

            bootstrapper = new AppBootstrapper();
        }

        public void HandleArguments(string[] args)
        {
            if (args.Length == 2)
            {
                var filePath = args[1];
                bootstrapper.GetEventAggregator().Publish(new FileOpenEvent(filePath));
            }
        }

        [STAThread]
        public static void Main()
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (directoryName != null && Directory.GetCurrentDirectory() != directoryName)
                Directory.SetCurrentDirectory(directoryName);

            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                new App().Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            HandleArguments(args.ToArray());

            return true;
        }
    }
}
