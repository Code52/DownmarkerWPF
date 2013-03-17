using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using MarkPad.Document.Events;
using MarkPad.Helpers;

namespace MarkPad.Document.EditorBehaviours
{
    public class PasteURLIntoDocument : IHandle<EditorPreviewKeyDownEvent>
    {
        // "URL: Find in full text (protocol optional)" from the RegexBuddy library, with ^$ anchors added
        readonly Regex URLInTextRegex = new Regex(@"^\b((?:(?:https?|ftp|file)://|www\.|ftp\.)[-A-Z0-9+&@#/%=~_|$?!:,.]*[A-Z0-9+&@#/%=~_|$]$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public void Handle(EditorPreviewKeyDownEvent e)
        {
            if (e.ViewModel == null) return;
            if (Keyboard.Modifiers != ModifierKeys.Control || e.Args.Key != Key.V) return;

            var pastedText = Clipboard.GetText();
            var match = URLInTextRegex.Match(pastedText);
            if (!match.Success) return;

            if (e.Editor.SelectionStart != 0 && (e.Editor.SelectionStart + e.Editor.SelectionLength) != e.Editor.Document.TextLength) // check if at beginning or end of document
            {
                if (e.Editor.Document.GetCharAt(e.Editor.SelectionStart - 1) == '[' && e.Editor.Document.GetCharAt(e.Editor.SelectionStart + e.Editor.SelectionLength) == ']')
                {
                    return;
                }
                if (e.Editor.Document.GetCharAt(e.Editor.SelectionStart - 1) == '"' && e.Editor.Document.GetCharAt(e.Editor.SelectionStart + e.Editor.SelectionLength) == '"')
                {
                    return;
                }
            }

            var oldSelectionStart = e.Editor.SelectionStart;
            e.Editor.TextArea.Selection.ReplaceSelectionWithText(match.Result("[$1]($1)"));
            e.Editor.SelectionStart = oldSelectionStart + match.Index + 1;
            e.Editor.SelectionLength = match.Length;
            e.Args.Handled = true;
        }
    }
}