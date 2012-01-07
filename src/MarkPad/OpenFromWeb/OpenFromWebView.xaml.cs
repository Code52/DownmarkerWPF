using System.Windows.Input;
using MarkPad.Metaweblog;

namespace MarkPad.OpenFromWeb
{
    public partial class OpenFromWebView
    {
        public Post OpenFromWeb()
        {
            InitializeComponent();

            return new Post();
        }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
                DragMove();
        }

        private void ContinueClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
