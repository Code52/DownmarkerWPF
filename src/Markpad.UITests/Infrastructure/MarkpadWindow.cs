using System;
using System.Windows.Automation;
using White.Core;
using White.Core.UIItems;
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
            WhiteWindow.Get<Button>("ShowNew").Click();
            WhiteWindow.Get<Button>("NewDocument").Click();

            Retry.For(() => "New Document" == CurrentDocument.Title, 5);
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
            Retry.For(ShellIsBusy, isBusy => isBusy, (int)TimeSpan.FromSeconds(30).TotalMilliseconds);
        }

        bool ShellIsBusy()
        {
            var currentPropertyValue = WhiteWindow.AutomationElement.GetCurrentPropertyValue(AutomationElement.HelpTextProperty);
            return currentPropertyValue != null && ((string)currentPropertyValue).Contains("Busy");
        }
    }
}