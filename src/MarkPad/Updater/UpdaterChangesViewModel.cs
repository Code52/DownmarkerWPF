using Caliburn.Micro;

namespace MarkPad.Updater
{
    public class UpdaterChangesViewModel : Screen
    {
        public string Message { get; set; }
        public bool WasCancelled { get; private set; }

        public UpdaterChangesViewModel()
        {
            WasCancelled = true;
        }

        public void Cancel()
        {
            TryClose();
        }

        public void Accept()
        {
            WasCancelled = false;
            TryClose();
        }
    }
}
