using White.Core;
using White.Core.UIItems;
using White.Core.UIItems.Finders;
using White.Core.UIItems.WindowItems;

namespace Markpad.UITests.Infrastructure
{
    public class MarkpadWindow : Screen
    {
        public MarkpadWindow(Application application, Window whiteWindow)
            :base(application, whiteWindow)
        { }

        public MarkpadWindow NewDocument()
        {
            WhiteWindow.Get<Button>("ShowNew").Click();
            WhiteWindow.Get<Button>("NewDocument").Click();

            return this;
        }

        public MarkpadDocument CurrentDocument
        {
            get
            {
                return new MarkpadDocument(this);
            }
        }

        public void Save()
        {
            WhiteWindow.Get<Button>("ShowSave").Click();
            WhiteWindow.Get<Button>("SaveDocument").Click();
        }

        public void SaveAs(string path)
        {
            WhiteWindow.Get<Button>("ShowSave").Click();
            WhiteWindow.Get<Button>("SaveAsDocument").Click();

            var modalWindow = WhiteWindow.ModalWindow("Save As");
            modalWindow.Get<TextBox>(SearchCriteria.ByAutomationId("1001")).Text = path;
            modalWindow.Get<Button>(SearchCriteria.ByAutomationId("1")).Click();
            WhiteWindow.WaitWhileBusy();
        }
    }
}