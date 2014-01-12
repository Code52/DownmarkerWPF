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

        public static string GetPrevCharacter(this TextEditor editor)
        {
            return GetPrevCharacter(editor, editor.CaretOffset);
        }

        public static string GetPrevCharacter(this TextEditor editor, int position)
        {
            if (position < 1) return "";
            return editor.Document.GetText(position - 1, 1);
        }

        public static string GetNextCharacter(this TextEditor editor)
        {
            return GetNextCharacter(editor, editor.CaretOffset);
        }

        public static string GetNextCharacter(this TextEditor editor, int position)
        {
            if (position < 0) return "";
            if (editor.Document.TextLength == position) return "";
            return editor.Document.GetText(position, 1);
        }

        /*public static void MoveToPrevInstanceOfString(TextEditor editor, string match)
        {
            if (match.Equals(TextEditorExtensions.GetNextCharacter(editor))) return;
            if (editor.IsCaratAtEndOfLine()) return;
            if (editor.CaretOffset >= editor.Document.TextLength) return;

            editor.CaretOffset++;

            MoveToPrevInstanceOfString(editor, match);
        }

        public static void MoveToNextInstanceOfString(TextEditor editor, string match)
        {
            if (match.Equals(TextEditorExtensions.GetNextCharacter(editor))) return;
            if (editor.IsCaratAtEndOfLine()) return;
            if (editor.CaretOffset >= editor.Document.TextLength) return;

            editor.CaretOffset++;

            MoveToNextInstanceOfString(editor, match);
        }*/
    }
}
