using System;
using System.IO;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit.Document;
using MarkdownSharp;
using MarkPad.Services.Interfaces;

namespace MarkPad.Document
{
    internal class DocumentViewModel : Screen
    {
        private readonly IDialogService dialogService;

        private string title;
        private string original;
        private TextDocument textDocument;
        private string filename;

        public DocumentViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            title = "New Document";
            original = "";
            textDocument = new TextDocument();
        }


        public void Open(string filename)
        {
            this.filename = filename;
            var text = File.ReadAllText(filename);
            title = new FileInfo(filename).Name;
            textDocument.Text = text;
            original = text;
        }

        public void Update()
        {
            
            OnDocumentChanged();
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

            File.WriteAllText(filename, Document.Text);
            original = textDocument.Text;

            OnDocumentChanged();
        }

        private void OnDocumentChanged()
        {
            NotifyOfPropertyChange(() => Render);
            NotifyOfPropertyChange(() => HasChanges);
            NotifyOfPropertyChange(() => DisplayName);
        }

        public TextDocument Document
        {
            get { return textDocument; }
            set
            {
                textDocument = value;
                OnDocumentChanged();
            }
        }

        public string Render
        {
            get
            {
                var markdown = new Markdown();

                return markdown.Transform(this.Document.Text);
            }
        }

        public bool HasChanges
        {
            get
            {
                return original != Document.Text;
            }
        }

        public override string DisplayName
        {
            get { return title + (HasChanges ? " *" : ""); }
            set { }
        }
    }
}
