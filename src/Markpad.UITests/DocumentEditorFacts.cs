using System.Windows.Forms;
using MarkPad.UITests.Infrastructure;
using White.Core.WindowsAPI;
using Xunit;

namespace MarkPad.UITests
{
    public class DocumentEditorTests
    {
        public class ListFacts : MarkpadUiTest
        {
            [Fact]
            public void EditorContinuesUnorderedList()
            {
                var document = MainWindow.NewDocument();

                var editor = document.Editor();
                editor.TypeText(" - List");
                editor.PressKey(KeyboardInput.SpecialKeys.RETURN);
                editor.TypeText("Continued");

                const string listContinued = " - List\r\n - Continued";

                Assert.Equal(listContinued, editor.MarkdownText);
            }

            [Fact]
            public void EditorContinuesOrderedListWithAllOnes()
            {
                var document = MainWindow.NewDocument();

                var editor = document.Editor();
                editor.TypeText("1. List");
                editor.PressKey(KeyboardInput.SpecialKeys.RETURN);
                editor.TypeText("Continued");

                const string listContinued = "1. List\r\n1. Continued";

                Assert.Equal(listContinued, editor.MarkdownText);
            }

            [Fact]
            public void EditorContinuesOrderedListWithNumberedList()
            {
                var document = MainWindow.NewDocument();

                var editor = document.Editor();
                editor.MarkdownText = "1. List\r\n2. Continued";
                editor.MoveCursorToEndOfEditor();
                editor.PressKey(KeyboardInput.SpecialKeys.RETURN);
                editor.TypeText("With count");

                const string listContinued = "1. List\r\n2. Continued\r\n3. With count";

                Assert.Equal(listContinued, editor.MarkdownText);
            }

            [Fact]
            public void TabIndentsListWithSpaces()
            {
                var document = MainWindow.NewDocument();
                var settingsWindow = MainWindow.Settings();
                settingsWindow.Indent = Indent.Spaces;
                settingsWindow.Close();

                var editor = document.Editor();
                editor.TypeText("1. List");
                editor.PressKey(KeyboardInput.SpecialKeys.RETURN);
                editor.PressKey(KeyboardInput.SpecialKeys.TAB);
                editor.TypeText("Continued");

                const string listContinued = "1. List\r\n    1. Continued";

                Assert.Equal(listContinued, editor.MarkdownText);
            }

            [Fact]
            public void TabIndentsListWithTabs()
            {
                var document = MainWindow.NewDocument();
                var settingsWindow = MainWindow.Settings();
                settingsWindow.Indent = Indent.Tabs;
                settingsWindow.Close();

                var editor = document.Editor();
                editor.EditorUIItem.Focus();
                editor.TypeText("1. List");
                editor.PressKey(KeyboardInput.SpecialKeys.RETURN);
                editor.PressKey(KeyboardInput.SpecialKeys.TAB);
                editor.TypeText("Continued");

                const string listContinued = "1. List\r\n\t1. Continued";

                Assert.Equal(listContinued, editor.MarkdownText);
            }

            [Fact]
            public void CreateLinkFromPastedURL()
            {
                const string textToPaste = "http://www.google.com";
                var document = MainWindow.NewDocument();
                var editor = document.Editor();

                Clipboard.SetText(textToPaste);
                document.PasteClipboard();

                Assert.Equal("[http://www.google.com](http://www.google.com)", editor.MarkdownText);

                // test caret position and text selection by pressing backspace
                editor.PressKey(KeyboardInput.SpecialKeys.BACKSPACE);
                Assert.Equal("[](http://www.google.com)", editor.MarkdownText);
            }

            [Fact]
            public void DontCreateLinkWhenPastingURLInsideExistingLink()
            {
                const string textToPaste = "http://www.google.com";
                var document = MainWindow.NewDocument();
                var editor = document.Editor();
                editor.MarkdownText = "[](http://www.google.com)";

                editor.PressKey(KeyboardInput.SpecialKeys.RIGHT);
                Clipboard.SetText(textToPaste);
                document.PasteClipboard();

                Assert.Equal("[http://www.google.com](http://www.google.com)", editor.MarkdownText);
            }

            [Fact]
            public void DontCreateLinkWhenPastingURLInsideQuotationMarks()
            {
                const string textToPaste = "http://www.google.com";
                var document = MainWindow.NewDocument();
                var editor = document.Editor();
                editor.MarkdownText = "\"\"";

                editor.PressKey(KeyboardInput.SpecialKeys.RIGHT);
                Clipboard.SetText(textToPaste);
                document.PasteClipboard();

                Assert.Equal("\"http://www.google.com\"", editor.MarkdownText);
            }
        }
    }
}
