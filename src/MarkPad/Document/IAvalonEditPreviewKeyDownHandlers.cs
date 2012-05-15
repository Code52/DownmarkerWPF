using System.Windows.Input;
using ICSharpCode.AvalonEdit;

namespace MarkPad.Document
{
    public interface IAvalonEditPreviewKeyDownHandlers
    {
        void Handle(DocumentViewModel viewModel, TextEditor editor, KeyEventArgs e);
    }
}
