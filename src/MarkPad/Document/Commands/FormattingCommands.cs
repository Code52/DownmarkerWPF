using System.Windows.Input;

namespace MarkPad.Document.Commands
{
    public class FormattingCommands
    {
        public static ICommand ToggleBold = new RoutedCommand();
        public static ICommand ToggleItalic = new RoutedCommand();
        public static ICommand ToggleCode = new RoutedCommand();
        public static ICommand ToggleCodeBlock = new RoutedCommand();
        public static ICommand SetHyperlink = new RoutedCommand();
    }
}
