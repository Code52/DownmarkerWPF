using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MarkPad.Shell
{
    public partial class ShellView
    {
        private bool CheatSheetVisible = false;
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