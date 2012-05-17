using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;
using System.Windows.Input;

namespace MarkPad.Document.AvalonEditPreviewKeyDownHandlers
{
    public class HardLineBreak : IAvalonEditPreviewKeyDownHandlers
    {
        public void Handle(DocumentViewModel viewModel, TextEditor editor, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Shift) return;
            if (e.Key != Key.Enter) return;

            editor.TextArea.Selection.ReplaceSelectionWithText("  " + Environment.NewLine);
            e.Handled = true;
        }
    }
}
