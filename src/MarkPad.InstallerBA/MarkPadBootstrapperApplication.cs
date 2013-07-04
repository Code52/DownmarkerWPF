using System.Threading;
using System.Windows.Threading;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

[assembly: BootstrapperApplication(typeof(MarkPad.InstallerBA.MarkPadBootstrapperApplication))]

namespace MarkPad.InstallerBA
{
    public class MarkPadBootstrapperApplication : BootstrapperApplication
    {
        // global dispatcher
        static public Dispatcher BootstrapperDispatcher { get; private set; }

        // entry point for our custom UI
        protected override void Run()
        {
            Engine.Log(LogLevel.Verbose, "Launching custom MarkPad Installer UX");

            BootstrapperDispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(BootstrapperDispatcher));

            MainViewModel viewModel = new MainViewModel(this);

            MainView view = new MainView();
            view.DataContext = viewModel;
            view.Closed += (sender, e) => BootstrapperDispatcher.InvokeShutdown();
            view.Closing += (sender, e) =>
            {
                e.Cancel = !viewModel.TryClose();
            };
            view.Show();

            Dispatcher.Run();

            Engine.Quit(0);
        }
    }
}