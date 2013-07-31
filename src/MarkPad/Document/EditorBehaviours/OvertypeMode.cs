using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using MarkPad.Document.Events;
using MarkPad.Events;

namespace MarkPad.Document.EditorBehaviours
{
    public class OvertypeMode : IHandle<EditorPreviewKeyDownEvent>, IHandle<EditorTextEnteringEvent>
    {
        public void Handle(EditorPreviewKeyDownEvent e)
        {
            if (e.Args.Key != Key.Insert || Keyboard.Modifiers != ModifierKeys.None) return;

            e.ViewModel.Overtype = !e.ViewModel.Overtype;
            e.Args.Handled = true;
        }

        public void Handle(EditorTextEnteringEvent e)
        {
            if (!e.ViewModel.Overtype) return;
            if (!e.Editor.TextArea.Selection.IsEmpty) return;
            if (e.Editor.Document.GetLineByNumber(e.Editor.TextArea.Caret.Line).EndOffset == e.Editor.CaretOffset) return;
            if (e.Args.Text.StartsWith("\r") || e.Args.Text.StartsWith("\n")) return;

            e.Editor.SelectionLength = 1;
        }
    }
}
