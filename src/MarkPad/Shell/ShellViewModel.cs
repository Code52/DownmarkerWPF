using System;
using System.IO;
using System.Windows.Shell;
using Caliburn.Micro;
using MarkPad.Document;
using MarkPad.Framework.Events;
using MarkPad.MDI;
using MarkPad.Services.Interfaces;
using MarkPad.Settings;

namespace MarkPad.Shell
{
    internal class ShellViewModel : Conductor<IScreen>, IHandle<AppStartedEvent>
    {
        private readonly IDialogService dialogService;
        private readonly IWindowManager windowService;
        private readonly Func<DocumentViewModel> documentCreator;
        private readonly Func<SettingsViewModel> settingsCreator;

        public ShellViewModel(
            IDialogService dialogService,
            IWindowManager windowService,
            MDIViewModel mdi,
            Func<DocumentViewModel> documentCreator,
            Func<SettingsViewModel> settingsCreator)
        {
            this.dialogService = dialogService;
            this.windowService = windowService;
            this.MDI = mdi;
            this.documentCreator = documentCreator;
            this.settingsCreator = settingsCreator;

            this.ActivateItem(mdi);
        }

        public override string DisplayName
        {
            get { return "MarkPad"; }
            set { }
        }

        public MDIViewModel MDI { get; private set; }

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
            var path = dialogService.GetFileOpenPath("Open a markdown document.", "Markdown Document (*.md)|*.md|Any File (*.*)|*.*");
            if (string.IsNullOrEmpty(path))
                return;

            OpenDocument(path);
        }

        public void OpenDocument(string path)
        {
            JumpList.AddToRecentCategory(path);

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

        public void ShowSettings()
        {
            windowService.ShowDialog(settingsCreator());
        }

        public void Handle(AppStartedEvent message)
        {
            if (message.Args.Length == 1)
            {
                if (File.Exists(message.Args[0]) && Path.GetExtension(message.Args[0]) == ".md")
                    OpenDocument(message.Args[0]);
            }
        }
    }
}
