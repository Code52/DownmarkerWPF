using System.Collections.Generic;
using System.Linq;
using MarkPad.Services.Events;

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
