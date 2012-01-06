using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using MarkPad.Framework.Events;

namespace MarkPad.Shell
{
    public partial class ShellView
    {
        private readonly IEventAggregator eventAggregator;

        public ShellView(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
                DragMove();
        }

        private void ButtonMinimiseOnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonMaxRestoreOnClick(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                maxRestore.Content = "1";
                WindowState = WindowState.Normal;
            }
            else
            {
                maxRestore.Content = "2";
                WindowState = WindowState.Maximized;
            }
        }

        internal void OpenFile(string path)
        {
            eventAggregator.Publish(new FileOpenEvent(path));
        }
    }
}