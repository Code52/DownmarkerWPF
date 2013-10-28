using TestStack.White;
using TestStack.White.UIItems.WindowItems;

namespace MarkPad.UITests.Infrastructure
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