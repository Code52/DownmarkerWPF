using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MarkPad.DocumentSources;
using MarkPad.Plugins;
using Microsoft.Expression.Interactivity.Core;

namespace MarkPad.Document.Controls
{
    public partial class SiteView
    {
        public static DependencyProperty SiteContextProperty =
            DependencyProperty.Register("SiteContext", typeof(ISiteContext), typeof(SiteView), new PropertyMetadata(default(ISiteContext)));

        SiteItemBase currentlySelectedItem;
        DateTime? selectedTime;
        readonly ContextMenu itemContextMenu;

        public SiteView()
        {
            InitializeComponent();

            itemContextMenu = new ContextMenu
            {
                Items =
                {
                    new MenuItem {Header = "Rename", Command = new ActionCommand(Rename)}, 
                    new MenuItem {Header = "Delete", Command = new ActionCommand(DeleteItem)}
                }
            };
        }

        void DeleteItem()
        {
            if (currentlySelectedItem != null)
                currentlySelectedItem.Delete();
        }

        void Rename()
        {
            if (currentlySelectedItem != null)
                currentlySelectedItem.IsRenaming = true;
        }

        public ISiteContext SiteContext
        {
            get { return (ISiteContext)GetValue(SiteContextProperty); }
            set { SetValue(SiteContextProperty, value); }
        }

        void SiteFilesMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = siteFiles.SelectedItem as SiteItemBase;

            if (selectedItem != null)
            {
                SetItemSelected(selectedItem, false);
                SiteContext.OpenItem(selectedItem);
                e.Handled = true;
            }
        }

        void SiteItemOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var textBlock = (TextBlock)sender;
            var siteItem = (SiteItemBase)textBlock.DataContext;
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                if (siteItem.Selected && selectedTime != null &&
                    DateTime.Now.Subtract(selectedTime.Value).TotalMilliseconds > 500)
                {
                    siteItem.IsRenaming = true;
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                SetItemSelected(siteItem, true);
                textBlock.ContextMenu = itemContextMenu;
                itemContextMenu.PlacementTarget = textBlock;
                itemContextMenu.IsOpen = true;
            }
        }

        void SetItemSelected(SiteItemBase siteItem, bool isSelected)
        {
            try
            {
                siteItem.Selected = isSelected;
                var treeViewItem = TreeViewHelper.GetTreeViewItem(siteFiles, siteItem);

                //uncomment the following line if UI updates are unnecessary
                treeViewItem.IsSelected = true;

                const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
                var selectMethod = typeof(TreeViewItem).GetMethod("Select", bindingFlags);

                selectMethod.Invoke(treeViewItem, new object[] { isSelected });
            }
            catch { }
        }


        void EditBoxKeyDown(object sender, KeyEventArgs e)
        {
            var textBlock = (TextBox)sender;
            var siteItem = (SiteItemBase)textBlock.DataContext;

            if (siteItem.IsRenaming)
            {
                if (e.Key == Key.Escape)
                {
                    siteItem.UndoRename();
                    e.Handled = true;
                }
                if (e.Key == Key.Enter)
                {
                    siteItem.CommitRename();
                    e.Handled = true;
                }
            }
        }

        private void SiteFilesSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (currentlySelectedItem != null)
                currentlySelectedItem.Selected = false;

            var siteItem = (SiteItemBase)e.NewValue;

            if (siteItem == null)
            {
                currentlySelectedItem = null;
                selectedTime = null;
                return;
            }

            siteItem.Selected = true;
            currentlySelectedItem = siteItem;
            selectedTime = DateTime.Now;
        }

        private void SiteFilesKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2 && currentlySelectedItem != null && !currentlySelectedItem.IsRenaming)
            {
                currentlySelectedItem.IsRenaming = true;
            }
        }

        public void UndoRename()
        {
            if (currentlySelectedItem != null && currentlySelectedItem.IsRenaming)
                currentlySelectedItem.IsRenaming = false;
        }
    }
}
