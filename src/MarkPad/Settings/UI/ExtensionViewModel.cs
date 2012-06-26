using Caliburn.Micro;

namespace MarkPad.Settings.UI
{
    public class ExtensionViewModel : PropertyChangedBase
    {
        public ExtensionViewModel(string extension, bool enabled)
        {
            Extension = extension;
            Enabled = enabled;
        }

        public string Extension { get; private set; }
        public bool Enabled { get; set; }
    }
}