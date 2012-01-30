using Caliburn.Micro;

namespace MarkPad.HyperlinkEditor
{
    public class HyperlinkEditorViewModel : Screen
    {
        public HyperlinkEditorViewModel(string text, string url)
        {
            Text = text;
            Url = url;
            WasCancelled = false;
        }

        public bool WasCancelled { get; private set; }

        public string Text { get; set; }
        public string Url { get; set; }

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
