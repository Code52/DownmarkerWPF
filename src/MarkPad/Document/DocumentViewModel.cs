using System.IO;
using Caliburn.Micro;
using MarkdownSharp;
using MarkPad.Services.Interfaces;

namespace MarkPad.Document
{
    internal class DocumentViewModel : Screen
    {
        private readonly IDialogService dialogService;

        private string title;
        private string original;
        private string document;
        private string filename;

        public DocumentViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            title = "New Document";
            original = "";
            Document = "";
        }

        public void Open(string filename)
        {
            this.filename = filename;
            var text = File.ReadAllText(filename);
            title = new FileInfo(filename).Name;
            document = text;
            original = text;
        }

        public void Save()
        {
            if (!HasChanges)
                return;

            if (string.IsNullOrEmpty(filename))
            {
                var path = dialogService.GetFileSavePath("Choose a location to save the document.", "*.md", "Markdown Files (*.md)|*.md|All Files (*.*)|*.*");

                if (string.IsNullOrEmpty(path))
                    return;

                filename = path;
                title = new FileInfo(filename).Name;
            }

            File.WriteAllText(filename, Document);
            original = document;

            OnDocumentChanged();
        }

        private void OnDocumentChanged()
        {
            NotifyOfPropertyChange(() => Render);
            NotifyOfPropertyChange(() => HasChanges);
            NotifyOfPropertyChange(() => DisplayName);
        }

        public string Document
        {
            get { return document; }
            set
            {
                document = value;
                OnDocumentChanged();
            }
        }

        public string Render
        {
            get
            {
                var markdown = new Markdown();

                return markdown.Transform(this.Document);
            }
        }

        public bool HasChanges
        {
            get
            {
                return original != Document;
            }
        }

        public override string DisplayName
        {
            get { return title + (HasChanges ? " *" : ""); }
            set { }
        }
    }
}
