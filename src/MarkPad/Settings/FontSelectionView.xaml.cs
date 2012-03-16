using System.Windows;
using System.Windows.Input;

namespace MarkPad.Settings
{
	public partial class FontSelectionView
	{
		public FontSelectionView()
		{
			InitializeComponent();
		}

		private void DragMoveWindow(object sender, MouseButtonEventArgs e)
		{
			if (e.RightButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
				DragMove();
		}
	}
}
