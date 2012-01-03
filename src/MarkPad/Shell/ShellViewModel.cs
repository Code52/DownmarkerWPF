using System;
using Caliburn.Micro;
using MarkPad.Document;
using MarkPad.MDI;

namespace MarkPad.Shell
{
    internal class ShellViewModel : Conductor<IScreen>
    {
        private Func<DocumentViewModel> documentCreator;

        public ShellViewModel(MDIViewModel mdi, Func<DocumentViewModel> documentCreator)
        {
            this.MDI = mdi;
            this.documentCreator = documentCreator;
        }

        public MDIViewModel MDI { get; private set; }

        public void Exit()
        {
            this.TryClose();
        }

        public void NewDocument()
        {
            MDI.Open(documentCreator());
        }
    }
}
