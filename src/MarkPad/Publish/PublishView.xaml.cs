namespace MarkPad.Publish
{
    public partial class PublishView
    {
        public PublishView()
        {
            InitializeComponent();
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
