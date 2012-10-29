using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MarkPad.Document.Commands;
using MarkPad.Framework;
using MarkPad.Plugins;
using MarkPad.PreviewControl;

namespace MarkPad
{
    public partial class ShellView
    {
        bool ignoreNextMouseMove;

        readonly IList<ICanCreateNewPage> canCreateNewPagePlugins;
        readonly IList<ICanSavePage> canSavePagePlugins;

        public ShellView(IEnumerable<ICanCreateNewPage> canCreateNewPagePlugins, IEnumerable<ICanSavePage> canSavePagePlugins)
        {
            this.canCreateNewPagePlugins = canCreateNewPagePlugins.ToList();
            this.canSavePagePlugins = canSavePagePlugins.ToList();

            CommandBindings.Add(new CommandBinding(ShellCommands.Esc, (x, y) => PressedEsc()));
            CommandBindings.Add(new CommandBinding(ShellCommands.Search, (x, y) => Search()));

            InitializeComponent();

			UpdatePlugins();
        }

		void UpdatePlugins()
		{
			CreateNewPageHook.Children.Clear();
			foreach (var plugin in canCreateNewPagePlugins.Where(p => p.Settings.IsEnabled))
			{
				var button = new Button { Content = plugin.CreateNewPageLabel.ToUpper(), Tag = plugin };
			    var capturedPlugin = plugin;
			    button.Click += (o, e) =>
				{
				    var text = capturedPlugin.CreateNewPage();
				    (DataContext as ShellViewModel).ExecuteSafely(vm => vm.NewDocument(text));
				};
				CreateNewPageHook.Children.Add(button);
			}

			SavePageHook.Children.Clear();
			foreach (var plugin in canSavePagePlugins.Where(p => p.Settings.IsEnabled))
			{
				var button = new Button { Content = plugin.SavePageLabel.ToUpper(), Tag = plugin };
			    var capturedPlugin = plugin;
			    button.Click += (sender, args) => (DataContext as ShellViewModel).ExecuteSafely(vm =>
				{
				    if (vm.ActiveDocumentViewModel == null) return;
				    capturedPlugin.SavePage(vm.ActiveDocumentViewModel.MarkpadDocument);
				});
				SavePageHook.Children.Add(button);
			}
		}

        bool DocumentIsOpen { get { return (DataContext as ShellViewModel).Evaluate(vm => vm.MDI.ActiveItem != null); } }

        void DragMoveWindow(object sender, MouseButtonEventArgs e)
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

        void MouseMoveWindow(object sender, MouseEventArgs e)
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

        void ToggleMaximized()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        void ShellViewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (DocumentIsOpen && !header.IsMouseOver) return;
            ToggleMaximized();
        }

        void ButtonMinimiseOnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        void ButtonMaxRestoreOnClick(object sender, RoutedEventArgs e)
        {
            ToggleMaximized();
        }

        protected override void OnStateChanged(System.EventArgs e)
        {
            RefreshMaximiseIconState();
            base.OnStateChanged(e);
        }

        void RefreshMaximiseIconState()
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

        void WindowDragOver(object sender, DragEventArgs e)
        {
            var isFileDrop = e.Data.GetDataPresent(DataFormats.FileDrop);
            e.Effects = isFileDrop ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HtmlPreview htmlPreview = ((ShellViewModel) DataContext).MDI.HtmlPreview;
            if (htmlPreview != null)
                htmlPreview.Close();
        }

        private void PressedEsc()
        {
            if (searchPanel.IsVisible)
            {
                searchPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void Search()
        {
            if (!searchPanel.IsVisible)
            {
                searchPanel.Visibility = Visibility.Visible;
            }

            searchTextBox.Focus();
            searchTextBox.SelectAll();
        }
    }
}
