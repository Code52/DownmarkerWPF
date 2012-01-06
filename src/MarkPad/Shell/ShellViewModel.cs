using System;
using Caliburn.Micro;
using MarkPad.Document;
using MarkPad.MDI;
using MarkPad.Metaweblog;
using MarkPad.OpenFromWeb;
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

            this.ActivateItem(mdi);
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

        public void NewJekyllDocument()
        {
            var creator = documentCreator();
            creator.Document.BeginUpdate();
            creator.Document.Text = CreateJekyllHeader();
            creator.Document.EndUpdate();
            MDI.Open(creator);
        }

        private static string CreateJekyllHeader()
        {
            var permalink = "new-page.html";
            var title = "New Post";
            var description = "Some Description";
            var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss zzz");
            return string.Format("---\r\nlayout: post\r\ntitle: {0}\r\npermalink: {1}\r\ndescription: {2}\r\ndate: {3}\r\ntags: \"some tags here\"\r\n---\r\n\r\n", title, permalink, description, date);
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

        public void OpenFromWeb()
        {
            var result = _windowManager.ShowDialog(new OpenFromWebViewModel(_settingsService));
            if (result != true)
                return;

            var post = _settingsService.Get<Post>("CurrentPost");
            
            _settingsService.Set(post.permalink, post);
            _settingsService.Save();

            var doc = documentCreator();
            doc.OpenFromWeb(post.permalink, post.description);
            MDI.Open(doc);
        }
    }
}
