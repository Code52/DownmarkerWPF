using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace MarkPad.Infrastructure
{
    public partial class ExceptionDialog
    {
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

        public Exception Exception { get; set; }

        private void TryClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            SetData(DataFormats.Text, Details);
        }

        // Implementation taken from the WinForms clipboard class.
        // Seriously, the clipboard can fail, so it retries 10 times.
        private void SetData(string format, object data)
        {
            if (!data.GetType().IsSerializable)
            {
                throw new NotSupportedException("An object being added to the clipboard must be serializable. Ensure that the entire object tree is serializable.");
            }

            bool succeeded = false;
            for (int i = 0; i < 10 && !succeeded; i++)
            {
                try
                {
                    Clipboard.SetData(format, data);
                    succeeded = true;
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e, "ERROR");
                }
                if (!succeeded)
                    Thread.Sleep(100);
            }
        }
    }
}
