using System.Windows.Input;
using ICSharpCode.AvalonEdit;

namespace MarkPad.Document.Events
{
    public class EditorPreviewKeyDownEvent
    {
        public DocumentViewModel ViewModel{get;private set;}
        public TextEditor Editor{get;private set;}
        public KeyEventArgs Args { get; private set; }

        public EditorPreviewKeyDownEvent(DocumentViewModel viewModel, TextEditor editor, KeyEventArgs args)
        {
            ViewModel = viewModel;
            Editor = editor;
            Args = args;
        }
    }
}
