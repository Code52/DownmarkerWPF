using System.Windows.Input;
using Caliburn.Micro;
using MarkPad.Document.Events;
using MarkPad.Events;

namespace MarkPad.Document.EditorBehaviours
{
    public class OvertypeMode : IHandle<EditorPreviewKeyDownEvent>, IHandle<EditorTextEnteringEvent>
    {
        bool _overtype = false;

        public void Handle(EditorPreviewKeyDownEvent e)
        {
            if (e.Args.Key != Key.Insert) return;

            _overtype = !_overtype;
            e.Args.Handled = true;

            // TODO: change cursor style - possible not supported in AvalonEdit
        }

        public void Handle(EditorTextEnteringEvent e)
        {
            if (!_overtype) return;
            if (!e.Editor.TextArea.Selection.IsEmpty) return;
            if (e.Editor.Document.GetLineByNumber(e.Editor.TextArea.Caret.Line).EndOffset == e.Editor.CaretOffset) return;
            if (e.Args.Text.StartsWith("\r") || e.Args.Text.StartsWith("\n")) return;

            e.Editor.SelectionLength = 1;
        }
    }
}
