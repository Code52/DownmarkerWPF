using System.Windows;
using System.Windows.Input;

namespace MarkPad.Settings
{
    /// <summary>
    /// Interaction logic for BlogSettingsView.xaml
    /// </summary>
    public partial class BlogSettingsView : Window
    {
        public BlogSettingsView()
        {
            InitializeComponent();
        }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
                DragMove();
        }

        private void OkClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
