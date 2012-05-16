using System.Text.RegularExpressions;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;

namespace MarkPad.Document.AvalonEditPreviewKeyDownHandlers
{
    public class ControlRightMovesToEndOfCurrentWord : IAvalonEditPreviewKeyDownHandlers
    {
        readonly Regex _wordRegex = new Regex(@"[\w]", RegexOptions.Compiled);
        readonly Regex _nonWordRegex = new Regex(@"[\W]", RegexOptions.Compiled);

        public void Handle(DocumentViewModel viewModel, TextEditor editor, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            if (e.Key != Key.Right) return;
            if (GetIsCaratAtEndOfLine(editor)) return;

            // If cursor is at the start or within a word, move to the end of that word.
            // Otherwise:
            //  If there is only whitespace between the cursor and the start of the next word, move to the end of the next word,
            //  otherwise move to the start of the next word.

            if (_wordRegex.IsMatch(GetNextLetter(editor)))
            {
                MoveToEndOfCurrentWord(editor);
            }
            else
            {
                var textBeforeNextWord = GetTextBeforeNextWord(editor);
                MoveToBeginningOfNextWord(editor);
                if (string.IsNullOrWhiteSpace(textBeforeNextWord))
                {
                    MoveToEndOfCurrentWord(editor);
                }
            }

            e.Handled = true;
            return;
        }

        bool GetIsCaratAtEndOfLine(TextEditor editor)
        {
            var currentLine = editor.Document.GetLineByOffset(editor.TextArea.Caret.Offset);
            return currentLine.EndOffset == editor.TextArea.Caret.Offset;
        }

        void MoveToEndOfCurrentWord(TextEditor editor)
        {
            if (GetIsCaratAtEndOfLine(editor)) return;
            if (editor.CaretOffset + 1 >= editor.Document.TextLength) return;

            editor.CaretOffset++;
            
            if (_nonWordRegex.IsMatch(GetNextLetter(editor))) return;
            
            MoveToEndOfCurrentWord(editor);
        }

        string GetTextBeforeNextWord(TextEditor editor)
        {
            var initialOffset = editor.CaretOffset;
            
            MoveToBeginningOfNextWord(editor);
            
            var textBeforeNextWord = editor.Document.GetText(initialOffset, editor.CaretOffset - initialOffset);
            
            editor.CaretOffset = initialOffset;
            
            return textBeforeNextWord;
        }


        void MoveToBeginningOfNextWord(TextEditor editor)
        {
            if (_wordRegex.IsMatch(GetNextLetter(editor))) return;
            if (GetIsCaratAtEndOfLine(editor)) return;
            if (editor.CaretOffset >= editor.Document.TextLength) return;

            editor.CaretOffset++;
            
            MoveToBeginningOfNextWord(editor);
        }

        string GetNextLetter(TextEditor editor)
        {
            if (editor.Document.TextLength == editor.CaretOffset) return "";
            return editor.Document.GetText(editor.CaretOffset, 1);
        }
    }
}
