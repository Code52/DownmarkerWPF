using System.Windows.Input;
using Caliburn.Micro;
using MarkPad.Document.Events;
using MarkPad.Events;

namespace MarkPad.Document.EditorBehaviours
{
    public class CursorLeftRightWithSelection : IHandle<EditorPreviewKeyDownEvent>
    {
        public void Handle(EditorPreviewKeyDownEvent e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift) return;
            if (e.Args.Key != Key.Left && e.Args.Key != Key.Right) return;
            if (e.Editor.SelectionLength == 0) return;

            var newOffset = e.Editor.SelectionStart;
            if (e.Args.Key == Key.Right) newOffset += e.Editor.SelectionLength;

            e.Editor.SelectionLength = 0;
            e.Editor.CaretOffset = newOffset;
            e.Args.Handled = true;
        }
    }
}
