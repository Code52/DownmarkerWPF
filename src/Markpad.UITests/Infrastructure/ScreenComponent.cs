namespace Markpad.UITests.Infrastructure
{
    public class ScreenComponent<T> where T : Screen
    {
        readonly T parentScreen;

        public ScreenComponent(T parentScreen)
        {
            this.parentScreen = parentScreen;
        }

        public T ParentScreen
        {
            get { return parentScreen; }
        }
    }
}