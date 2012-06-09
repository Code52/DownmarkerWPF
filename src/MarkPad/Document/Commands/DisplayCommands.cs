using System.Windows.Input;

namespace MarkPad.Document.Commands
{
	public static class DisplayCommands
	{
		public static ICommand ZoomIn = new RoutedCommand();
		public static ICommand ZoomOut = new RoutedCommand();
		public static ICommand ZoomReset = new RoutedCommand();
	}
}
