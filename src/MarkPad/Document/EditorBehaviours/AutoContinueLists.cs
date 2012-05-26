using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit;
using MarkPad.Framework.Events;

namespace MarkPad.Document.EditorBehaviours
{
    public class AutoContinueLists : IHandle<EditorPreviewKeyDownEvent>
    {
        // [\s]*    zero or more whitespace chars
        // [-\*]    one of '-' or '*'
        // [\s]+    one or more whitespace chars
        readonly Regex _unorderedListRegex = new Regex(@"[\s]*[-\*][\s]+", RegexOptions.Compiled);

        // [\s]*    zero or more whitespace chars
        // [0-9]+   one or more of 0-9
        // [.]      a single period
        // [\s]+    one or more whitespace chars
        readonly Regex _orderedListRegex = new Regex(@"[\s]*[0-9]+[.][\s]+", RegexOptions.Compiled);

        // [\s]*    zero or more whitespace chars
        // [>]      a single raquo
        // [\s]*    zero or more whitespace chars
        readonly Regex _quoteRegex = new Regex(@"[\s]*[>][\s]*", RegexOptions.Compiled);

        public void Handle(EditorPreviewKeyDownEvent e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;
            if (e.Args.Key != Key.Enter) return;
            
            // should really handle shift-enter too (insert hard break, indent, then on enter restart list)...

            if (!e.Editor.IsCaratAtEndOfLine()) return;

            var handled = false;

            handled = handled || HandleUnorderedList(e.Editor);
            handled = handled || HandleOrderedList(e.Editor);
            handled = handled || HandleQuote(e.Editor);

            e.Args.Handled = handled;
        }

        bool HandleUnorderedList(TextEditor editor)
        {
            var match = _unorderedListRegex.Match(editor.GetTextLeftOfCursor());
            if (!match.Success) return false;

            if (match.Value == editor.GetTextLeftOfCursor()) EndList(editor);
            else editor.TextArea.Selection.ReplaceSelectionWithText(Environment.NewLine + match.Value);

            return true;
        }

        bool HandleOrderedList(TextEditor editor)
        {
            var match = _orderedListRegex.Match(editor.GetTextLeftOfCursor());
            if (!match.Success) return false;

            if (match.Value == editor.GetTextLeftOfCursor())
            {
                EndList(editor);
            }
            else
            {
                var index = 1;
                var indexText = match.Value.Replace(".", "").Trim();
                var canParse = int.TryParse(indexText, out index);
                var nextLine = match.Value;
                if (canParse) nextLine = match.Value.Replace(indexText, (index+1).ToString());

                editor.TextArea.Selection.ReplaceSelectionWithText(Environment.NewLine + nextLine);
            }

            return true;
        }

        bool HandleQuote(TextEditor editor)
        {
            var match = _quoteRegex.Match(editor.GetTextLeftOfCursor());
            if (!match.Success) return false;

            if (match.Value == editor.GetTextLeftOfCursor()) EndList(editor);
            else editor.TextArea.Selection.ReplaceSelectionWithText(Environment.NewLine + match.Value);

            return true;
        }

        private static void EndList(TextEditor editor)
        {
            var currentPosition = editor.TextArea.Caret.Offset;
            editor.SelectionStart = editor.GetCurrentLine().Offset;
            editor.SelectionLength = currentPosition - editor.GetCurrentLine().Offset;
            editor.TextArea.Selection.ReplaceSelectionWithText(Environment.NewLine);
        }
    }
}
