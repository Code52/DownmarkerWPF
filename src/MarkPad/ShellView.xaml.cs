using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using MarkPad.Events;
using MarkPad.Framework;
using MarkPad.Infrastructure.Plugins;
using MarkPad.Plugins;

namespace MarkPad
{
    public partial class ShellView : IHandle<PluginsChangedEvent>
    {
		readonly IPluginManager pluginManager;
        bool ignoreNextMouseMove;

		[ImportMany]
		IEnumerable<ICanCreateNewPage> canCreateNewPagePlugins;
		[ImportMany]
		IEnumerable<ICanSavePage> canSavePagePlugins;

        public ShellView(IPluginManager pluginManager)
        {
			this.pluginManager = pluginManager;
			this.pluginManager.Container.ComposeParts(this);

            InitializeComponent();

			UpdatePlugins();
        }

		void UpdatePlugins()
		{
			CreateNewPageHook.Children.Clear();
			foreach (var plugin in canCreateNewPagePlugins.Where(p => p.Settings.IsEnabled))
			{
				var button = new Button { Content = plugin.CreateNewPageLabel.ToUpper(), Tag = plugin };
				button.Click += (o, e) =>
				{
				    var text = plugin.CreateNewPage();
				    (DataContext as ShellViewModel).ExecuteSafely(vm => vm.NewDocument(text));
				};
				CreateNewPageHook.Children.Add(button);
			}

			SavePageHook.Children.Clear();
			foreach (var plugin in canSavePagePlugins.Where(p => p.Settings.IsEnabled))
			{
				var button = new Button { Content = plugin.SavePageLabel.ToUpper(), Tag = plugin };
				button.Click += (sender, args) => (DataContext as ShellViewModel).ExecuteSafely(vm =>
				{
				    if (vm.ActiveDocumentViewModel == null) return;
				    plugin.SavePage(vm.ActiveDocumentViewModel);
				});
				SavePageHook.Children.Add(button);
			}
		}

		public void Handle(PluginsChangedEvent e)
		{
			UpdatePlugins();
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
    }
}
