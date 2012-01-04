using System;
using Caliburn.Micro;
using MarkPad.Document;
using MarkPad.MDI;
using MarkPad.Services.Interfaces;

namespace MarkPad.Shell
{
    internal class ShellViewModel : Conductor<IScreen>
    {
        private readonly IDialogService dialogService;
        private readonly Func<DocumentViewModel> documentCreator;

        public ShellViewModel(IDialogService dialogService, MDIViewModel mdi, Func<DocumentViewModel> documentCreator)
        {
            this.dialogService = dialogService;
            this.MDI = mdi;
            this.documentCreator = documentCreator;

            this.ActivateItem(mdi);
        }

        public override string DisplayName
        {
            get { return "MarkPad"; }
            set { }
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
            var path = dialogService.GetFileOpenPath("Open a markdown document.", "Any File (*.*)|*.*");
            if (string.IsNullOrEmpty(path))
                return;

            var doc = documentCreator();
            doc.Open(path);
            MDI.Open(doc);
        }

        public void SaveDocument()
        {
            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                doc.Save();
            }
        }

        public void SaveAllDocuments()
        {
            foreach (DocumentViewModel doc in MDI.Items)
            {
                doc.Save();
            }
        }
    }
}
