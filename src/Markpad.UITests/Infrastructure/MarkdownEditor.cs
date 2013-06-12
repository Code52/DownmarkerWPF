using System.Threading;
using System.Windows.Automation;
using White.Core.UIItems;
using White.Core.Utility;
using White.Core.WindowsAPI;

namespace MarkPad.UITests.Infrastructure
{
    public class MarkdownEditor : ScreenComponent<MarkpadWindow>
    {
        public MarkdownEditor(MarkpadWindow parentScreen, IUIItem editorControl) : base(parentScreen)
        {
            EditorUIItem = editorControl;
        }

        public IUIItem EditorUIItem { get; private set; }

        public string MarkdownText
        {
            get
            {
                var valuePattern = (ValuePattern)EditorUIItem.AutomationElement.GetCurrentPattern(ValuePattern.Pattern);
                return valuePattern.Current.Value;
            }
            set
            {
                var valuePattern = (ValuePattern)EditorUIItem.AutomationElement.GetCurrentPattern(ValuePattern.Pattern);
                valuePattern.SetValue(value);
            }
        }

        public void TypeText(string list)
        {
            if (!EditorUIItem.IsFocussed)
                EditorUIItem.Focus();

            ParentScreen.WhiteWindow.Keyboard.Enter(list);
            Retry.ForDefault(() => MarkdownText, s => !s.Contains(list));
        }

        public void PressKey(KeyboardInput.SpecialKeys @return)
        {
            ParentScreen.WhiteWindow.Keyboard.PressSpecialKey(@return);
        }

        public void MoveCursorToEndOfEditor()
        {
            ParentScreen.WhiteWindow.Keyboard.HoldKey(KeyboardInput.SpecialKeys.CONTROL);
            ParentScreen.WhiteWindow.Keyboard.PressSpecialKey(KeyboardInput.SpecialKeys.END);
            ParentScreen.WhiteWindow.Keyboard.LeaveKey(KeyboardInput.SpecialKeys.CONTROL);
        }
    }
}