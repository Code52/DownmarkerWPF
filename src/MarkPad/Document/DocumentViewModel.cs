using System.IO;
using Caliburn.Micro;
using MarkdownSharp;
using Microsoft.Win32;

namespace MarkPad.Document
{
    internal class DocumentViewModel : Screen
    {
        private string title;
        private string original;
        private string document;
        private string _filename;

        public DocumentViewModel()
        {
            title = "New Document";
            original = "";
            Document = "";
        }

        public void Open(string filename)
        {
            _filename = filename;
            var text = File.ReadAllText(filename);
            title = new FileInfo(filename).Name;
            document = text;
            original = text;
        }
        public void Save()
        {
            if (!HasChanges)
                return;

            if (string.IsNullOrEmpty(_filename))
            {
                var saveDialog = new SaveFileDialog();
                if (saveDialog.ShowDialog() == false)
                    return;

                _filename = saveDialog.FileName;
                title = new FileInfo(_filename).Name;
            }

            File.WriteAllText(_filename, Document);
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
