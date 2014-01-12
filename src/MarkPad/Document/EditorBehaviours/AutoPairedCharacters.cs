using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit;
using MarkPad.Document.Events;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;
using System.Linq;

namespace MarkPad.Document.EditorBehaviours
{
    class AutoPairedCharacters : IHandle<EditorTextEnteredEvent>, IHandle<EditorTextEnteringEvent>, IHandle<EditorPreviewKeyDownEvent>
    {
        private static Dictionary<string, string> pairedChars = new Dictionary<string, string>()
        {
            {"(",")"},
            {"[","]"},
            {"{","}"},
            {"'","'"},
            {"\"","\""},
            {"<",">"}
        };

        private PairedCharacterRenderer pairedCharacterRenderer;
        private HashSet<string> validFollowingChars;

        public AutoPairedCharacters()
        {
            pairedCharacterRenderer = new PairedCharacterRenderer();
            validFollowingChars = new HashSet<string>() { "", " " };
            pairedChars.Values.ToList().ForEach(v => validFollowingChars.Add(v));
        }

        public void Handle(EditorTextEnteredEvent e)
        {
        }

        public void Handle(EditorTextEnteringEvent e)
        {
            if (pairedChars.Any(pc => pc.Value == e.Args.Text) && e.Editor.GetNextCharacter() == e.Args.Text
                && GetOpeningCharPosition(e.Args.Text, e.Editor.CaretOffset, e.Editor) != -1)
            {
                //type 'over' the next character
                e.Editor.SelectionLength = 1;
            }
            else if (ValidInsertPosition(e.Editor) && pairedChars.ContainsKey(e.Args.Text))
            {
                //insert the paired character
                var charactedToInsert = pairedChars[e.Args.Text];
                e.Editor.TextArea.Selection.ReplaceSelectionWithText(charactedToInsert);
                e.Editor.CaretOffset -= charactedToInsert.Length;
            }
        }

        public void Handle(EditorPreviewKeyDownEvent e)
        {
            if (e.Args.Key == Key.Back && e.Editor.CaretOffset > 0)
            {
                var textToRemove = e.Editor.Document.GetText(e.Editor.CaretOffset - 1, 1);

                if (pairedChars.ContainsKey(textToRemove) && e.Editor.GetNextCharacter() == pairedChars[textToRemove])
                {
                    //remove the paired character
                    e.Editor.Select(e.Editor.CaretOffset, 1);
                    e.Editor.TextArea.Selection.ReplaceSelectionWithText("");
                }
            }
        }

        public static bool IsValidOpeningCharacter(string character)
        {
            return pairedChars.ContainsKey(character);
        }

        public static bool IsValidClosingCharacter(string character)
        {
            return pairedChars.ContainsValue(character);
        }

        public static int GetClosingCharPosition(string openingChar, int startAt, TextEditor editor)
        {
            var closeChar = pairedChars[openingChar];
            int count = 0;
            var startingPosition = startAt - 1;

            var nextChar = "";
            do
            {
                startingPosition++;
                nextChar = editor.GetNextCharacter(startingPosition);
                
                if (nextChar == closeChar)
                    count--;
                else if (nextChar == openingChar)
                    count++;

            } while ((nextChar != closeChar || count > -1) && nextChar != "");

            return startingPosition < editor.Document.TextLength ? startingPosition : -1;
        }

        public static int GetOpeningCharPosition(string closingChar, int startAt, TextEditor editor)
        {
            var openChar = pairedChars.Single(pc => pc.Value == closingChar).Key;
            int count = 0;
            var startingPosition = startAt;

            var nextChar = "";
            do
            {
                startingPosition--;
                nextChar = editor.GetNextCharacter(startingPosition);

                if (nextChar == openChar)
                    count--;
                else if (nextChar == closingChar)
                    count++;

            } while ((nextChar != openChar || count > -1) && nextChar != "");

            return startingPosition >= 0 ? startingPosition : -1;
        }

        private bool ValidInsertPosition(TextEditor editor)
        {
            return validFollowingChars.Contains(editor.GetNextCharacter());
        }
    }
}
