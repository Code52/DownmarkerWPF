using Markpad.UITests.Infrastructure;
using White.Core.WindowsAPI;
using Xunit;

namespace Markpad.UITests
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

                const string listContinued = @" - List
 - Continued";

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

                const string listContinued = @"1. List
1. Continued";

                Assert.Equal(listContinued, editor.MarkdownText);
            }

            [Fact]
            public void EditorContinuesOrderedListWithNumberedList()
            {
                var document = MainWindow.NewDocument();

                var editor = document.Editor();
                editor.MarkdownText = @"1. List
2. Continued";
                editor.MoveCursorToEndOfEditor();
                editor.PressKey(KeyboardInput.SpecialKeys.RETURN);
                editor.TypeText("With count");

                const string listContinued = @"1. List
2. Continued
3. With count";

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

                const string listContinued = @"1. List
    1. Continued";

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

                const string listContinued = @"1. List
	1. Continued";

                Assert.Equal(listContinued, editor.MarkdownText);
            }
        }
    }
}
