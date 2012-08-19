using System.Text.RegularExpressions;
using System.Windows.Input;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit;
using MarkPad.Document.Events;
using MarkPad.Events;

namespace MarkPad.Document.EditorBehaviours
{
    public class IndentLists : IHandle<EditorPreviewKeyDownEvent>
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

        public void Handle(EditorPreviewKeyDownEvent e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;
            if (e.Args.Key != Key.Tab) return;
            
           // if (!e.Editor.IsCaratAtEndOfLine()) return;

            var handled = false;

            handled = handled || HandleUnorderedList(e.Editor);
            handled = handled || HandleOrderedList(e.Editor);

            e.Args.Handled = handled;
        }

        bool HandleUnorderedList(TextEditor editor)
        {
            var match = _unorderedListRegex.Match(editor.GetTextLeftOfCursor());
            if (!match.Success) return false;
            if (match.Value != editor.GetTextLeftOfCursor()) return false;

            InsertIndent(editor);

            return true;
        }

        bool HandleOrderedList(TextEditor editor)
        {
            var match = _orderedListRegex.Match(editor.GetTextLeftOfCursor());
            if (!match.Success) return false;
            if (match.Value != editor.GetTextLeftOfCursor()) return false;

            InsertIndent(editor);

            return true;
        }

        private static void InsertIndent(TextEditor editor)
        {
            var currentOffset = editor.CaretOffset;
            editor.CaretOffset = editor.GetCurrentLine().Offset;
            editor.TextArea.Selection.ReplaceSelectionWithText("\t");
            editor.CaretOffset = currentOffset + 1;
        }
    }
}
