using White.Core;
using White.Core.UIItems.WindowItems;

namespace Markpad.UITests.Infrastructure
{
    public class Screen
    {
        protected Screen(Application application, Window whiteWindow)
        {
            Application = application;
            WhiteWindow = whiteWindow;
        }

        public Application Application { get; set; }
        public Window WhiteWindow { get; set; }
    }
}