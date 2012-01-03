using System.IO;
using Caliburn.Micro;
using MarkdownSharp;

namespace MarkPad.Document
{
    internal class DocumentViewModel : Screen
    {
        private string title;
        private readonly string original;
        private string document;

        public DocumentViewModel()
        {
            title = "New Document";
            original = "";
            Document = "";
        }

        public void Open(string filename)
        {
            var text = File.ReadAllText(filename);
            title = new FileInfo(filename).Name;
            document = text;
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
