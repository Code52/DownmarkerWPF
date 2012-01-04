using System;
using Caliburn.Micro;
using MarkPad.Document;
using MarkPad.MDI;
using MarkPad.Publish;
using MarkPad.Services.Interfaces;

namespace MarkPad.Shell
{
    internal class ShellViewModel : Conductor<IScreen>
    {
        private readonly IDialogService dialogService;
        private readonly IWindowManager _windowManager;
        private readonly ISettingsService _settingsService;
        private readonly Func<DocumentViewModel> documentCreator;

        public ShellViewModel(IDialogService dialogService, MDIViewModel mdi, IWindowManager windowManager, ISettingsService settingsService, Func<DocumentViewModel> documentCreator)
        {
            this.dialogService = dialogService;
            _windowManager = windowManager;
            _settingsService = settingsService;
            this.MDI = mdi;
            this.documentCreator = documentCreator;
        }

        public override string DisplayName
        {
            get { return "MarkPad"; }
            set { }
        }

        public MDIViewModel MDI { get; private set; }

        public override void CanClose(Action<bool> callback)
        {
            base.CanClose(callback);
            _settingsService.Save();
        }

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

        public void Publish()
        {
            if (string.IsNullOrEmpty(_settingsService.Get<string>("BlogUrl")))
            { 
                var result =_windowManager.ShowDialog(new PublishViewModel(_settingsService));
                if (result != true)
                    return;
            }

            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                doc.Publish();
            }
        }
    }
}
