using System.Windows.Input;
using MarkPad.Infrastructure.DialogService;

namespace MarkPad.DocumentSources.MetaWeblog
{
    public partial class PublishDetailsView
    {
        private readonly IDialogService dialogService;

        public PublishDetailsView(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            InitializeComponent();
        }

        private void ContinueClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PostTitle.Text))
            {
                dialogService.ShowWarning("Error Publishing Post", "Post title needs to be entered before publishing.", "");
                return;
            }

            DialogResult = true;
        }

        private void CancelClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
                DragMove();
        }
    }
}
