using Caliburn.Micro;

namespace MarkPad
{
    internal class MDIViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public void Open(IScreen screen)
        {
            this.ActivateItem(screen);
        }
    }
}
