using System;
using System.Windows.Input;
using Caliburn.Micro;
using MarkPad.Document.Events;
using MarkPad.Events;

namespace MarkPad.Document.EditorBehaviours
{
    public class CopyLeadingWhitespaceOnNewLine : IHandle<EditorPreviewKeyDownEvent>
    {
        public void Handle(EditorPreviewKeyDownEvent e)
        {
            if (e.Args.Key != Key.Enter) return;

            var line = e.Editor.Document.GetLineByOffset(e.Editor.CaretOffset);
            
            if (line.Length == 0) return;
            if (e.Editor.CaretOffset != line.Offset) return;

            var lineText = e.Editor.Document.GetText(line.Offset, line.Length);
            var trimmedLineText = lineText.TrimStart('\t', ' ');
            
            if (lineText.Length == trimmedLineText.Length) return;
            
            var leadingWhitespace = lineText.Substring(0, lineText.Length - trimmedLineText.Length);
            e.Editor.TextArea.Selection.ReplaceSelectionWithText(leadingWhitespace + Environment.NewLine);
            e.Args.Handled = true;
        }
    }
}
