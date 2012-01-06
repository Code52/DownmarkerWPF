namespace MarkPad.Framework.Events
{
    public class AppStartedEvent
    {
        private readonly string[] args;

        public AppStartedEvent(string[] args)
        {
            this.args = args;
        }

        public string[] Args
        {
            get { return args; }
        }
    }
}
