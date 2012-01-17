using System.Windows;
using System.Windows.Input;

namespace MarkPad
{
    /// <summary>
    /// Interaction logic for ExceptionDialog.xaml
    /// </summary>
    public partial class ExceptionDialog : Window
    {
        private System.Exception exception;
        private string details;
        private string message;

        public ExceptionDialog()
        {
            InitializeComponent();
        }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
                DragMove();
        }

        public string Message
        {
            get { return message; }
            set
            {
                message = value;

                messageBox.Text = value;
            }
        }

        public string Details
        {
            get { return details; }
            set
            {
                details = value;
                detailsBox.Text = value;
            }
        }

        public System.Exception Exception
        {
            get { return exception; }
            set { exception = value; }
        }

        private void TryClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
