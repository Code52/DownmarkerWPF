using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using MarkPad.Document.AvalonEditPreviewKeyDownHandlers;
using Xunit;

namespace MarkPad.Tests.Document.AvalonEditPreviewKeyDownHandlers
{
    public class CursorLeftRightWithSelectionTests
    {
        public class FakePresentationSource : PresentationSource
        {
            protected override System.Windows.Media.CompositionTarget GetCompositionTargetCore()
            {
                return null;
            }

            public override System.Windows.Media.Visual RootVisual { get; set; }
            public override bool IsDisposed { get { return false; } }
        }

        KeyEventArgs GetKeyEventArgs(Key key)
        {
            return new KeyEventArgs(Keyboard.PrimaryDevice, new FakePresentationSource(), 0, key)
            {
                RoutedEvent = UIElement.KeyDownEvent
            };
        }

        [Fact]
        public void sets_to_left_of_selection_on_cursor_left()
        {
            var editor = new TextEditor()
            {
                Text = "this is a test",
                SelectionStart = 3,
                SelectionLength = 5,
                CaretOffset = 8
            };

            new CursorLeftRightWithSelection().Handle(null, editor, GetKeyEventArgs(Key.Left));

            Assert.Equal(3, editor.CaretOffset);
        }

        [Fact]
        public void sets_to_right_of_selection_on_cursor_right()
        {
            var editor = new TextEditor
            {
                Text = "this is a test",
                SelectionStart = 3,
                SelectionLength = 5,
                CaretOffset = 8
            };

            new CursorLeftRightWithSelection().Handle(null, editor, GetKeyEventArgs(Key.Right));

            Assert.Equal(8, editor.CaretOffset);
        }
    }
}
