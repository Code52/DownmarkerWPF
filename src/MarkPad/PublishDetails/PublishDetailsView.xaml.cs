using System.Windows;
using System.Windows.Input;

namespace MarkPad.PublishDetails
{
    public partial class PublishDetailsView
    {
        public PublishDetailsView()
        {
            InitializeComponent();
        }

        private void ContinueClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(PostTitle.Text))
            {
                MessageBox.Show("Post title needs to be entered before publishing.", "Error Publishing Post", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void CancelClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
                DragMove();
        }
    }
}
