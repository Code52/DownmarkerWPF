using System.Windows;
using System.Windows.Input;

namespace MarkPad.HyperlinkEditor
{
    /// <summary>
    /// Interaction logic for HyperlinkEditorView.xaml
    /// </summary>
    public partial class HyperlinkEditorView : Window
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
