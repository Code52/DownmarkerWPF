using System.IO;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.TabItems;
using TestStack.White.WindowsAPI;

namespace MarkPad.UITests.Infrastructure
{
    public class MarkpadDocument : ScreenComponent<MarkpadWindow>
    {
        public MarkpadDocument(MarkpadWindow markpadWindow) : base(markpadWindow)
        { }

        public string Title
        {
            get
            {
                return ParentScreen.WhiteWindow.Get<Tab>("Items")
                                                 .SelectedTab
                                                 .GetElement(SearchCriteria.ByAutomationId("DocumentTitle"))
                                                 .Current.Name;
            }
        }

        public int Index
        {
            get
            {
                var tab = ParentScreen.WhiteWindow.Get<Tab>("Items");
                return tab.Pages.IndexOf(tab.SelectedTab);
            }
        }

        public MarkpadDocument Save()
        {
            ParentScreen.WhiteWindow.Get<Button>("ShowSave").Click();
            ParentScreen.WhiteWindow.Get<Button>("SaveDocument").Click();

            return this;
        }

        public MarkpadDocument SaveAs(string path)
        {
            var directoryName = Path.GetDirectoryName(path);

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            ParentScreen.WhiteWindow.Get<Button>("ShowSave").Click();
            ParentScreen.WhiteWindow.Get<Button>("SaveAsDocument").Click();

            var modalWindow = ParentScreen.WhiteWindow.ModalWindow("Save As");
            modalWindow.Get<TextBox>(SearchCriteria.ByAutomationId("1001")).Text = path;
            modalWindow.Get<Button>(SearchCriteria.ByAutomationId("1")).Click();

            ParentScreen.WaitWhileBusy();

            return this;
        }

        public MarkpadDocument PasteClipboard()
        {
            Editor().EditorUIItem.Focus();
            ParentScreen.WhiteWindow.Keyboard.HoldKey(KeyboardInput.SpecialKeys.CONTROL);
            ParentScreen.WhiteWindow.Keyboard.Enter("v");
            ParentScreen.WhiteWindow.Keyboard.LeaveKey(KeyboardInput.SpecialKeys.CONTROL);

            return this;
        }

        public MarkdownEditor Editor()
        {
            var editorControl = ParentScreen.WhiteWindow.Get(SearchCriteria.ByAutomationId("Editor"));
            return new MarkdownEditor(ParentScreen, editorControl);
        }
    }
}