using System.Text.RegularExpressions;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace MarkPad.Document.AvalonEditPreviewKeyDownHandlers
{
    public class ControlRightTweakedForMarkdown : IAvalonEditPreviewKeyDownHandlers
    {
        readonly Regex _wordRegex = new Regex(@"[\w]", RegexOptions.Compiled);
        readonly Regex _nonWordRegex = new Regex(@"[\W]", RegexOptions.Compiled);
        readonly Regex _nonWhitespaceRegex = new Regex(@"[\S]", RegexOptions.Compiled);

        public void Handle(DocumentViewModel viewModel, TextEditor editor, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            if (e.Key != Key.Right) return;
            
            /* This is a tweaked variation of the behaviour in Sublime Text 2. It likes to find words, but
             * also likes control characters at the start of a line, which is a bit nicer for Markdown.
             * It picks the LHS (column zero) before traversing whitespace, which gives a bit more
             * control.
             * 
             * If the carat is at the end of the line, move to the start of the next line,
             * otherwise if the next character is word-like (a-z,A-Z,0-9,_), move to the end of the current word,
             * otherwise if the text to the left of the cursor is empty or whitespace:
             *      if the next character is not whitespace, move to the beginning of the next word,
             *      otherwise move to the next non-whitespace character
             * otherwise:
             *      record the text before the beginning of the next word, then
             *      move to the beginning of the next word, then
             *      if the recorded text is null or whitespace, move to the end of the current word
             * */

            var textLeftOfCursor = GetTextLeftOfCursor(editor);
            if (GetIsCaratAtEndOfLine(editor))
            {
                editor.CaretOffset = GetCurrentLine(editor).NextLine.Offset;
            }
            else if (_wordRegex.IsMatch(GetNextCharacter(editor)))
            {
                MoveToEndOfCurrentWord(editor);
            }
            else if (string.IsNullOrWhiteSpace(textLeftOfCursor))
            {
                if (_nonWhitespaceRegex.IsMatch(GetNextCharacter(editor)))
                {
                    MoveToBeginningOfNextWord(editor);
                }
                else
                {
                    MoveToFirstNonwhitespaceCharacter(editor);
                }
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
            return GetCurrentLine(editor).EndOffset == editor.TextArea.Caret.Offset;
        }

        void MoveToEndOfCurrentWord(TextEditor editor)
        {
            if (GetIsCaratAtEndOfLine(editor)) return;
            if (editor.CaretOffset + 1 >= editor.Document.TextLength) return;

            editor.CaretOffset++;
            
            if (_nonWordRegex.IsMatch(GetNextCharacter(editor))) return;
            
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

        string GetTextLeftOfCursor(TextEditor editor)
        {
            var currentLine = GetCurrentLine(editor);
            return editor.Document.GetText(currentLine.Offset, editor.CaretOffset - currentLine.Offset);
        }

        DocumentLine GetCurrentLine(TextEditor editor)
        {
            return editor.Document.GetLineByOffset(editor.TextArea.Caret.Offset);
        }

        void MoveToBeginningOfNextWord(TextEditor editor)
        {
            if (_wordRegex.IsMatch(GetNextCharacter(editor))) return;
            if (GetIsCaratAtEndOfLine(editor)) return;
            if (editor.CaretOffset >= editor.Document.TextLength) return;

            editor.CaretOffset++;
            
            MoveToBeginningOfNextWord(editor);
        }

        void MoveToFirstNonwhitespaceCharacter(TextEditor editor)
        {
            if (_nonWhitespaceRegex.IsMatch(GetNextCharacter(editor))) return;
            if (GetIsCaratAtEndOfLine(editor)) return;
            if (editor.CaretOffset >= editor.Document.TextLength) return;

            editor.CaretOffset++;

            MoveToFirstNonwhitespaceCharacter(editor);
        }

        string GetNextCharacter(TextEditor editor)
        {
            if (editor.Document.TextLength == editor.CaretOffset) return "";
            return editor.Document.GetText(editor.CaretOffset, 1);
        }
    }
}
