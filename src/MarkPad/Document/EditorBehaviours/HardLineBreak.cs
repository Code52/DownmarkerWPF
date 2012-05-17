using System;
using System.Windows.Input;
using Caliburn.Micro;
using MarkPad.Framework.Events;

namespace MarkPad.Document.AvalonEditPreviewKeyDownHandlers
{
    public class HardLineBreak : IHandle<EditorPreviewKeyDownEvent>
    {
        public void Handle(EditorPreviewKeyDownEvent e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Shift) return;
            if (e.Args.Key != Key.Enter) return;

            e.Editor.TextArea.Selection.ReplaceSelectionWithText("  " + Environment.NewLine);
            e.Args.Handled = true;
        }
    }
}
