using White.Core;
using White.Core.UIItems;
using White.Core.UIItems.WindowItems;

namespace Markpad.UITests.Infrastructure
{
    public class MarkpadWindow : Screen
    {
        public MarkpadWindow(Application application, Window whiteWindow)
            :base(application, whiteWindow)
        {
            
        }

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
    }
}