using System;
using Caliburn.Micro;
using MarkPad.Document;
using MarkPad.MDI;
using Microsoft.Win32;

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

        public void OpenDocument()
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != true)
                return;

            var doc = documentCreator();
            doc.Open(ofd.FileName);
            MDI.Open(doc);
        }
    }
}
