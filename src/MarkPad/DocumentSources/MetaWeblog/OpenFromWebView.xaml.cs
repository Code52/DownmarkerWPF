using System.Windows.Input;
using MarkPad.DocumentSources.MetaWeblog.Service;

namespace MarkPad.DocumentSources.MetaWeblog
{
    public partial class OpenFromWebView
    {
        public Post OpenFromWeb()
        {
            InitializeComponent();

            return new Post();
        }

        private void DragMoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
                DragMove();
        }
    }
}
