using System.Windows;
using System.Windows.Input;
using MarkPad.Framework;
using MarkPad.PluginApi;
using System.Collections.Generic;
using System.Windows.Controls;
using MarkPad.Framework;
using System.Linq;
using System.ComponentModel.Composition;

namespace MarkPad.Shell
{
    public partial class ShellView
    {
		readonly IPluginManager _pluginManager;

		[ImportMany]
		IEnumerable<ICanCreateNewPage> _canCreateNewPagePlugins;

        public ShellView(IPluginManager pluginManager)
        {
            InitializeComponent();

			_pluginManager = pluginManager;
			_pluginManager.Container.ComposeParts(this);

			UpdatePlugins();
        }

		void UpdatePlugins()
		{
			NewPageHook.Children.Clear();
			foreach (var plugin in _canCreateNewPagePlugins)
			{
				var button = new Button { Content = plugin.CreateNewPageLabel.ToUpper(), Tag = plugin };
				button.Click += new RoutedEventHandler((o, e) =>
				{
					var text = plugin.CreateNewPage();
					(DataContext as ShellViewModel).ExecuteSafely(vm => vm.NewDocument(text));
				});
				NewPageHook.Children.Add(button);
			}
		}

        private bool ignoreNextMouseMove;

        bool DocumentIsOpen { get { return (DataContext as ShellViewModel).Evaluate(vm => vm.MDI.ActiveItem != null); } }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed) return;
            if (e.RightButton == MouseButtonState.Pressed) return;
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (DocumentIsOpen && !header.IsMouseOver) return;

            if (WindowState == WindowState.Maximized && e.ClickCount != 2) return;

            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                ignoreNextMouseMove = true;
                return;
            }

            DragMove();
        }

        private void MouseMoveWindow(object sender, MouseEventArgs e)
        {
            if (ignoreNextMouseMove)
            {
                ignoreNextMouseMove = false;
                return;
            }

            if (WindowState != WindowState.Maximized) return;

            if (e.MiddleButton == MouseButtonState.Pressed) return;
            if (e.RightButton == MouseButtonState.Pressed) return;
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (!header.IsMouseOver) return;

            // Calculate correct left coordinate for multi-screen system
            var mouseX = PointToScreen(Mouse.GetPosition(this)).X;
            var width = RestoreBounds.Width;
            var left = mouseX - width / 2;
            if (left < 0) left = 0;

            // Align left edge to fit the screen
            var virtualScreenWidth = SystemParameters.VirtualScreenWidth;
            if (left + width > virtualScreenWidth) left = virtualScreenWidth - width;

            Top = 0;
            Left = left;

            WindowState = WindowState.Normal;

            DragMove();
        }

        private void ToggleMaximized()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        void ShellView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (DocumentIsOpen && !header.IsMouseOver) return;
            ToggleMaximized();
        }

        private void ButtonMinimiseOnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonMaxRestoreOnClick(object sender, RoutedEventArgs e)
        {
            ToggleMaximized();
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
    }
}
