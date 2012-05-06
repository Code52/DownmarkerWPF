using Caliburn.Micro;

namespace MarkPad.MDI
{
    internal class MDIViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public void Open(IScreen screen)
        {
            this.ActivateItem(screen);
        }

        /// <summary>
        /// Gets value indicating whether any document is opened thus active.
        /// </summary>
        public bool IsDocumentActive
        {
            get { return ActiveItem != null; }
        }

        public override void ActivateItem(IScreen item)
        {
            base.ActivateItem(item);
            NotifyOfPropertyChange("IsDocumentActive");
        }

        public override void DeactivateItem(IScreen item, bool close)
        {
            base.DeactivateItem(item, close);
            NotifyOfPropertyChange("IsDocumentActive");
        }

    }
}
