using System;
using Caliburn.Micro;
using MarkPad.Document;
using MarkPad.Framework.Events;
using MarkPad.MDI;
using MarkPad.Metaweblog;
using MarkPad.OpenFromWeb;
using MarkPad.Publish;
using MarkPad.PublishDetails;
using MarkPad.Services.Interfaces;
using MarkPad.Settings;

namespace MarkPad.Shell
{
    internal class ShellViewModel : Conductor<IScreen>, IHandle<FileOpenEvent>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IDialogService dialogService;
        private readonly IWindowManager _windowManager;
        private readonly ISettingsService _settingsService;
        private readonly Func<DocumentViewModel> documentCreator;
        private readonly Func<SettingsViewModel> settingsCreator;

        public ShellViewModel(
            IDialogService dialogService,
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            MDIViewModel mdi,
            Func<DocumentViewModel> documentCreator,
            Func<SettingsViewModel> settingsCreator)
        {
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            _windowManager = windowManager;
            _settingsService = settingsService;
            this.MDI = mdi;
            this.documentCreator = documentCreator;
            this.settingsCreator = settingsCreator;

            ActivateItem(mdi);
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
            const string permalink = "new-page.html";
            const string title = "New Post";
            const string description = "Some Description";
            var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss zzz");
            return string.Format("---\r\nlayout: post\r\ntitle: {0}\r\npermalink: {1}\r\ndescription: {2}\r\ndate: {3}\r\ntags: \"some tags here\"\r\n---\r\n\r\n", title, permalink, description, date);
        }

        public void OpenDocument()
        {
            var path = dialogService.GetFileOpenPath("Open a markdown document.", Constants.ExtensionFilter + "|Any File (*.*)|*.*");
            if (string.IsNullOrEmpty(path))
                return;

            eventAggregator.Publish(new FileOpenEvent(path));
        }

        public void OpenDocument(string path)
        {
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

        public void Handle(FileOpenEvent message)
        {
            OpenDocument(message.Path);
        }

        public void ShowSettings()
        {
            windowService.ShowDialog(settingsCreator());
        }

        public void PrintDocument()
        {
            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                doc.Print();
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
                var pd = new Details {Title = doc.Post.title, Categories = doc.Post.categories };
                var detailsResult = _windowManager.ShowDialog(new PublishDetailsViewModel(pd));
                if (detailsResult != true)
                    return;

                doc.Publish(pd.Title, pd.Categories);
            }
        }

        public void OpenFromWeb()
        {
            var result = _windowManager.ShowDialog(new OpenFromWebViewModel(_settingsService));
            if (result != true)
                return;

            var post = _settingsService.Get<Post>("CurrentPost");
            
            var doc = documentCreator();
            doc.OpenFromWeb(post);
            MDI.Open(doc);
        }
    }
}
