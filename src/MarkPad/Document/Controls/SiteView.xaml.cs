using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MarkPad.DocumentSources;

namespace MarkPad.Document.Controls
{
    public partial class SiteView
    {
        public static DependencyProperty SiteContextProperty = 
            DependencyProperty.Register("SiteContext", typeof (ISiteContext), typeof (SiteView), new PropertyMetadata(default(ISiteContext)));

        SiteItemBase currentlySelectedItem;

        public SiteView()
        {
            InitializeComponent();
        }

        public ISiteContext SiteContext
        {
            get { return (ISiteContext) GetValue(SiteContextProperty); }
            set { SetValue(SiteContextProperty, value); }
        }

        void SiteFilesMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = siteFiles.SelectedItem as SiteItemBase;

            if (selectedItem != null)
                SiteContext.OpenItem(selectedItem);
        }

        void SiteItemOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var textBlock = (TextBlock) sender;
            var siteItem = (SiteItemBase)textBlock.DataContext;

            if (siteItem.Selected)
            {
                siteItem.IsRenaming = true;
            }
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
            if (currentlySelectedItem!= null)
                currentlySelectedItem.Selected = false;

            var siteItem = (SiteItemBase) e.NewValue;

            siteItem.Selected = true;
            currentlySelectedItem = siteItem;
        }

        private void SiteFilesKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2 && currentlySelectedItem != null && !currentlySelectedItem.IsRenaming)
            {
                currentlySelectedItem.IsRenaming = true;
            }
        }
    }
}
