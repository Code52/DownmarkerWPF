using System.Threading;
using TestStack.White;
using TestStack.White.Configuration;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.ListBoxItems;
using TestStack.White.UIItems.WPFUIItems;
using TestStack.White.UIItems.WindowItems;

namespace MarkPad.UITests.Infrastructure
{
    public class SettingsWindow : Screen
    {
        readonly UIItem settingsControl;

        public SettingsWindow(Application application, Window whiteWindow, UIItem settingsControl)
            : base(application, whiteWindow)
        {
            this.settingsControl = settingsControl;
        }

        public string Indent
        {
            get { return settingsControl.Get<ComboBox>(SearchCriteria.ByAutomationId("IndentType")).SelectedItemText; }
            set
            {
                CoreAppXmlConfiguration.Instance.ComboBoxItemsPopulatedWithoutDropDownOpen = true;

                var comboBox = settingsControl.Get<WPFComboBox>(SearchCriteria.ByAutomationId("IndentType"));
                comboBox.Select(value);
            }
        }

        public void Close()
        {
            settingsControl.Get<Button>(SearchCriteria.ByAutomationId("CloseSettings")).Click();
            Thread.Sleep(300); // Wait 0.3 seconds for animation to finish
        }
    }
}