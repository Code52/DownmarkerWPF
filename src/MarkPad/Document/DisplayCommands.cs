using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MarkPad.Document
{
	public static class DisplayCommands
	{
		public static ICommand ZoomIn = new RoutedCommand();
		public static ICommand ZoomOut = new RoutedCommand();
	}
}
