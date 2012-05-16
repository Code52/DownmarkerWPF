using System.Windows.Input;
using ICSharpCode.AvalonEdit;

namespace MarkPad.Document.AvalonEditPreviewKeyDownHandlers
{
    public class CursorLeftRightWithSelection : IAvalonEditPreviewKeyDownHandlers
    {
        public void Handle(DocumentViewModel viewModel, TextEditor editor, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift) return;
            if (e.Key != Key.Left && e.Key != Key.Right) return;
            if (editor.SelectionLength == 0) return;

            var newOffset = editor.SelectionStart;
            if (e.Key == Key.Right) newOffset += editor.SelectionLength;

            editor.SelectionLength = 0;
            editor.CaretOffset = newOffset;
            e.Handled = true;
        }
    }
}
