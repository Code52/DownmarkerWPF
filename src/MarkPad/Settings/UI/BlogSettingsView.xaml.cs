using System.Windows;
using System.Windows.Input;

namespace MarkPad.Settings.UI
{
    /// <summary>
    /// Interaction logic for BlogSettingsView.xaml
    /// </summary>
    public partial class BlogSettingsView : Window
    {
        public BlogSettingsView()
        {
            InitializeComponent();

            CurrentBlog_Password.Loaded += CurrentBlog_Password_Loaded;
        }

        void CurrentBlog_Password_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as BlogSettingsViewModel;
            if (vm == null)
                return;

            CurrentBlog_Password.Password = vm.CurrentBlog.Password;
        }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
                DragMove();
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
