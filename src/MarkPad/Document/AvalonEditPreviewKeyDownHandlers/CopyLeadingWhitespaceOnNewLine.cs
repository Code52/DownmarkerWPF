using System;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;

namespace MarkPad.Document.AvalonEditPreviewKeyDownHandlers
{
    public class CopyLeadingWhitespaceOnNewLine : IAvalonEditPreviewKeyDownHandlers
    {
        public void Handle(DocumentViewModel viewModel, TextEditor editor, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var line = editor.Document.GetLineByOffset(editor.CaretOffset);
            
            if (line.Length == 0) return;
            if (editor.CaretOffset != line.Offset) return;

            var lineText = editor.Document.GetText(line.Offset, line.Length);
            var trimmedLineText = lineText.TrimStart('\t', ' ');
            
            if (lineText.Length == trimmedLineText.Length) return;
            
            var leadingWhitespace = lineText.Substring(0, lineText.Length - trimmedLineText.Length);
            editor.TextArea.Selection.ReplaceSelectionWithText(editor.TextArea, leadingWhitespace + Environment.NewLine);
            e.Handled = true;
        }
    }
}
