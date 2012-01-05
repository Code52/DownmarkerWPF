using System;
using System.IO;
using System.Windows.Shell;
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

        protected override void OnActivate()
        {
            base.OnActivate();

            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                if (File.Exists(args[1]) && Path.GetExtension(args[1]) == ".md")
                    OpenDocument(args[1]);
            }
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
    }
}
