using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MarkPad.Framework.Events;
using MarkPad.Services.Events;
using Microsoft.Shell;

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
                if (File.Exists(filePath) && Constants.DefaultExtensions.Contains(Path.GetExtension(filePath).ToLower()))
                    bootstrapper.GetEventAggregator().Publish(new FileOpenEvent(filePath));
            }
        }

        public static void Start()
        {
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
