using System.Windows.Input;

namespace MarkPad.Document.Commands
{
    public static class ShellCommands
    {
        public static ICommand Esc = new RoutedCommand();
        public static ICommand Search = new RoutedCommand();
    }
}