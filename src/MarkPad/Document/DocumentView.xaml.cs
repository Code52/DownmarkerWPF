using System.Reflection;
using System.Windows;
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

        private void DocumentViewLoaded(object sender, RoutedEventArgs e)
        {
            using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream("MarkPad.Syntax.Markdown.xshd"))
            using (var reader = new XmlTextReader(stream))
            {
                Document.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
        }
    }
}
