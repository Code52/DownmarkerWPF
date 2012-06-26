using System.Windows.Input;

namespace MarkPad.Document.Controls
{
    public partial class HyperlinkEditorView
    {
        public HyperlinkEditorView()
        {
            InitializeComponent();
        }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
                DragMove();
        }
    }
}
