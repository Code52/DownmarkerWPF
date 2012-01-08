using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using Awesomium.Core;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MarkPad.Framework;

namespace MarkPad.Document
{
    public partial class DocumentView
    {
        private ScrollViewer documentScrollViewer;
        public DocumentView()
        {
            InitializeComponent();
            Loaded += DocumentViewLoaded;
            wb.Loaded += WbLoaded;
        }

        void WbLoaded(object sender, RoutedEventArgs e)
        {
            wb.ExecuteJavascript("window.scrollTo(0," + documentScrollViewer.VerticalOffset + ");");
        }

        private void DocumentViewLoaded(object sender, RoutedEventArgs e)
        {
            using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream("MarkPad.Syntax.Markdown.xshd"))
            using (var reader = new XmlTextReader(stream))
            {
                Document.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            documentScrollViewer = Document.FindVisualChild<ScrollViewer>();

            if (documentScrollViewer != null)
            {
                documentScrollViewer.ScrollChanged += (i, j) => wb.ExecuteJavascript("window.scrollTo(0," + j.VerticalOffset + ");");
                var x = ((DocumentViewModel)DataContext);
                x.Document.TextChanged += (i, j) =>
                                              {
                                                  wb.LoadCompleted += (k, l) => wb.ExecuteJavascript("window.scrollTo(0," + documentScrollViewer.VerticalOffset + ");");
                                              };
            }

            
        }


        internal void ToggleBold()
        {
            var selectedText = GetSelectedText();
            if (string.IsNullOrWhiteSpace(selectedText)) return;

            Document.SelectedText = selectedText.ToggleBold(!selectedText.IsBold());
        }

        internal void ToggleItalic()
        {
            var selectedText = GetSelectedText();
            if (string.IsNullOrWhiteSpace(selectedText)) return;

            Document.SelectedText = selectedText.ToggleItalic(!selectedText.IsItalic());
        }

        private string GetSelectedText()
        {
            var textArea = Document.TextArea;
            if (textArea.Selection.IsEmpty)
                return null;
            //{
            //    var line = textArea.Document.GetLineByOffset(textArea.Caret.Offset);
            //    textArea.Selection = textArea.Selection.StartSelectionOrSetEndpoint(line.Offset, line.Offset + line.Length);
            //}

            return textArea.Selection.GetText(textArea.Document);
        }


    }


}
