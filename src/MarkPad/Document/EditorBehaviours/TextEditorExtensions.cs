using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace MarkPad.Document.EditorBehaviours
{
    public static class TextEditorExtensions
    {
        public static bool IsCaratAtEndOfLine(this TextEditor editor)
        {
            return GetCurrentLine(editor).EndOffset == editor.TextArea.Caret.Offset;
        }

        public static DocumentLine GetCurrentLine(this TextEditor editor)
        {
            return editor.Document.GetLineByOffset(editor.TextArea.Caret.Offset);
        }

        public static string GetTextLeftOfCursor(this TextEditor editor)
        {
            var currentLine = editor.GetCurrentLine();
            return editor.Document.GetText(currentLine.Offset, editor.CaretOffset - currentLine.Offset);
        }

    }
}
