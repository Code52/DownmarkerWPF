using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace MarkPad.Document
{
    public partial class DocumentView
    {
        public DocumentView()
        {
            InitializeComponent();
            Loaded += DocumentViewLoaded;
        }

        void DocumentViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {

            using (var reader = new XmlTextReader("Markdown.xshd"))
            {
                Document.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
        }
    }
}
