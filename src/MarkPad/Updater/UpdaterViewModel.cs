using Caliburn.Micro;

namespace MarkPad.Updater
{
    public class UpdaterViewModel : PropertyChangedBase
    {
        public int Progress { get; private set; }

        public UpdateState UpdateState { get; set; }

        public bool Background { get; set; }

        public UpdaterViewModel()
        {
            UpdateState = Updater.UpdateState.UpToDate;
            Background = false;
        }
    }
}