using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

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
                Editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            documentScrollViewer = FindVisualChild<ScrollViewer>(Editor);

            if (documentScrollViewer != null)
            {
                documentScrollViewer.ScrollChanged += (i, j) => wb.ExecuteJavascript("window.scrollTo(0," + j.VerticalOffset + ");");
                Editor.TextChanged += (i, j) =>
                                              {
                                                  wb.LoadCompleted += (k, l) => wb.ExecuteJavascript("window.scrollTo(0," + documentScrollViewer.VerticalOffset + ");");
                                              };
            }


        }

        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }


}
