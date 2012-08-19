using System.Windows;
using System.Windows.Controls;

namespace MarkPad.Settings.UI
{
    public partial class SettingsView
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        void ScrollToSelection(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox) sender;

            listBox.ScrollIntoView(listBox.SelectedItem);
        }
    }
}
