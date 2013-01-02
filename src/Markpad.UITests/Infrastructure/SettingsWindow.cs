using System.Threading;
using White.Core;
using White.Core.Configuration;
using White.Core.UIItems;
using White.Core.UIItems.Finders;
using White.Core.UIItems.ListBoxItems;
using White.Core.UIItems.WPFUIItems;
using White.Core.UIItems.WindowItems;

namespace Markpad.UITests.Infrastructure
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