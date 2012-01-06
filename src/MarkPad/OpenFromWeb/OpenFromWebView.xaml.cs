using MarkPad.Metaweblog;

namespace MarkPad.OpenFromWeb
{
    public partial class OpenFromWebView
    {
        public Post OpenFromWeb()
        {
            InitializeComponent();

            return new Post();
        }

        private void ContinueClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
