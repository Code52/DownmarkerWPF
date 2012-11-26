using System;
using System.Windows.Automation;
using White.Core;
using White.Core.UIItems;
using White.Core.UIItems.Actions;
using White.Core.UIItems.Custom;
using White.Core.UIItems.Finders;
using White.Core.UIItems.WindowItems;
using White.Core.Utility;

namespace Markpad.UITests.Infrastructure
{
    public class MarkpadWindow : Screen
    {
        public MarkpadWindow(Application application, Window whiteWindow)
            :base(application, whiteWindow)
        { }

        public MarkpadDocument NewDocument()
        {
            var index = CurrentDocument == null ? -1 : CurrentDocument.Index;
            WhiteWindow.Get<Button>("ShowNew").Click();
            var button = WhiteWindow.Get<Button>("NewDocument");
            button.Click();

            Retry.For(() => index != CurrentDocument.Index, TimeSpan.FromSeconds(5));
            WaitWhileBusy();

            return new MarkpadDocument(this);
        }

        public MarkpadDocument OpenDocument(string existingDoc)
        {
            WhiteWindow.Get<Button>("ShowOpen").Click();
            WhiteWindow.Get<Button>("OpenDocument").Click();

            var openDocumentWindow = WhiteWindow.ModalWindow("Open a markdown document.");
            openDocumentWindow.Get<TextBox>(SearchCriteria.ByAutomationId("1148")).Text = existingDoc;
            openDocumentWindow.Get<Button>(SearchCriteria.ByAutomationId("1")).Click();

            WaitWhileBusy();
            return new MarkpadDocument(this);
        }

        public MarkpadDocument CurrentDocument
        {
            get
            {
                return new MarkpadDocument(this);
            }
        }

        public void WaitWhileBusy()
        {
            Retry.For(ShellIsBusy, isBusy => isBusy, TimeSpan.FromSeconds(30));
        }

        bool ShellIsBusy()
        {
            var currentPropertyValue = WhiteWindow.AutomationElement.GetCurrentPropertyValue(AutomationElement.HelpTextProperty);
            return currentPropertyValue != null && ((string)currentPropertyValue).Contains("Busy");
        }

        public SettingsWindow Settings()
        {
            WhiteWindow.Get<Button>("MarkpadSettings").Click();
            var automationElement = WhiteWindow.GetElement(SearchCriteria.ByAutomationId("SettingsControl"));
            var settingsControl = new UIItemContainer(automationElement, new ProcessActionListener(automationElement));

            return new SettingsWindow(Application, WhiteWindow, settingsControl);
        }
    }
}