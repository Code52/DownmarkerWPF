namespace Markpad.UITests.Infrastructure
{
    public class ScreenComponent<T> where T : Screen
    {
        readonly T screen;

        public ScreenComponent(T screen)
        {
            this.screen = screen;
        }

        public T Screen
        {
            get { return screen; }
        }
    }
}