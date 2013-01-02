using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit;
using MarkPad.Document.Events;
using MarkPad.Settings.Models;

namespace MarkPad.Document.EditorBehaviours
{
    public class IndentLists : IHandle<EditorPreviewKeyDownEvent>
    {
        readonly Func<IndentType> indentTypeSetting;

        // [\s]*    zero or more whitespace chars
        // [-\*]    one of '-' or '*'
        // [\s]+    one or more whitespace chars
        readonly Regex unorderedListRegex = new Regex(@"[\s]*[-\*][\s]+", RegexOptions.Compiled);

        // [\s]*    zero or more whitespace chars
        // [0-9]+   one or more of 0-9
        // [.]      a single period
        // [\s]+    one or more whitespace chars
        readonly Regex orderedListRegex = new Regex(@"[\s]*[0-9]+[.][\s]+", RegexOptions.Compiled);

        public IndentLists(Func<IndentType> indentTypeSetting)
        {
            this.indentTypeSetting = indentTypeSetting;
        }

        public void Handle(EditorPreviewKeyDownEvent e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;
            if (e.Args.Key != Key.Tab) return;

            e.Args.Handled = HandleUnorderedList(e.Editor) || HandleOrderedList(e.Editor);
        }

        bool HandleUnorderedList(TextEditor editor)
        {
            var match = unorderedListRegex.Match(editor.GetTextLeftOfCursor());
            if (!match.Success) return false;
            if (match.Value != editor.GetTextLeftOfCursor()) return false;

            InsertIndent(editor);

            return true;
        }

        bool HandleOrderedList(TextEditor editor)
        {
            var match = orderedListRegex.Match(editor.GetTextLeftOfCursor());
            if (!match.Success) return false;
            if (match.Value != editor.GetTextLeftOfCursor()) return false;

            InsertIndent(editor);

            return true;
        }

        private void InsertIndent(TextEditor editor)
        {
            var currentOffset = editor.CaretOffset;
            editor.CaretOffset = editor.GetCurrentLine().Offset;
            var tabCharacter = indentTypeSetting() == IndentType.Tabs ? "\t" : "    ";
            editor.TextArea.Selection.ReplaceSelectionWithText(tabCharacter);
            editor.CaretOffset = currentOffset + tabCharacter.Length;
        }
    }
}
