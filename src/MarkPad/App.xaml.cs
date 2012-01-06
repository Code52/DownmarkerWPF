using System.Threading;
using System.Windows;

namespace MarkPad
{
    public partial class App
    {
        private Mutex mutex;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool isNewInstance;
            mutex = new Mutex(true, "Markpad.SingleInstanceCheck", out isNewInstance);
            if (!isNewInstance)
            {
                // TODO: send message to origianl app to open new tab
                Current.Shutdown();
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (mutex != null)
            {
                mutex.ReleaseMutex();
            }
        }
    }
}
