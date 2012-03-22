using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MarkPad.Shell
{
    public partial class ShellView
    {
        private bool CheatSheetVisible = false;

        public ShellView()
        {
            InitializeComponent();
        }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed && e.LeftButton == MouseButtonState.Pressed)
            {
                if (WindowState == WindowState.Maximized)
                {
                    // Calcualting correct left coordinate for multi-screen system.
                    var mouseX = PointToScreen(Mouse.GetPosition(this)).X;
                    var width = RestoreBounds.Width;
                    var left = mouseX - width / 2;

                    // Aligning window's position to fit the screen.
                    var virtualScreenWidth = SystemParameters.VirtualScreenWidth;
                    left = left < 0 ? 0 : left;
                    left = left + width > virtualScreenWidth ? virtualScreenWidth - width : left;

                    Top = 0;
                    Left = left;

                    // Restore window to normal state.
                    WindowState = WindowState.Normal;
                }

                DragMove();
            }
            if (e.ClickCount != 2)
                return;

            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void ButtonMinimiseOnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonMaxRestoreOnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        protected override void OnStateChanged(System.EventArgs e)
        {
            RefreshMaximiseIconState();
            base.OnStateChanged(e);
        }

        private void RefreshMaximiseIconState()
        {
            if (WindowState == WindowState.Normal)
            {
                maxRestore.Content = "1";
                maxRestore.SetResourceReference(ToolTipProperty, "WindowCommandsMaximiseToolTip");
            }
            else
            {
                maxRestore.Content = "2";
                maxRestore.SetResourceReference(ToolTipProperty, "WindowCommandsRestoreToolTip");
            }
        }

        private void WindowDragOver(object sender, DragEventArgs e)
        {
            var isFileDrop = e.Data.GetDataPresent(DataFormats.FileDrop);
            e.Effects = isFileDrop ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F9)
            {
                ToggleCheatSheet();
            }
        }

        private void ToggleCheatSheet()
        {
            if (CheatSheetVisible)
            {
                var sb = Resources["HideCheatSheet"] as Storyboard;
                if (sb == null)
                    return;
                sb.Begin();
            }
            else
            {
                var sb = Resources["ShowCheatSheet"] as Storyboard;
                if (sb == null)
                    return;
                sb.Begin();
            }

            CheatSheetVisible = !CheatSheetVisible;
        }

        private void DismissCheatSheet(object sender, RoutedEventArgs e)
        {
            ToggleCheatSheet();
        }
    }
}
