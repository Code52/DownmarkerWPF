using System.ComponentModel;
using Caliburn.Micro;

namespace MarkPad.Updater
{
    public class UpdaterChangesViewModel : Screen
    {
        public string Message { get; set; }
        public bool WasCancelled { get; private set; }

        public void Cancel()
        {
            WasCancelled = true;
            TryClose();
        }

        public void Accept()
        {
            TryClose();
        }
    }
}
