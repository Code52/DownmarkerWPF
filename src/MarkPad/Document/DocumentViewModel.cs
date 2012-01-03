using Caliburn.Micro;
using MarkdownSharp;

namespace MarkPad.Document
{
    internal class DocumentViewModel : Screen
    {
        private readonly string title;
        private readonly string original;
        private string document;

        public DocumentViewModel()
        {
            title = "New Document";
            original = "";
            Document = "";
        }

        private void OnDocumentChanged()
        {
            NotifyOfPropertyChange(() => this.Render);
            NotifyOfPropertyChange(() => this.HasChanges);
            NotifyOfPropertyChange(() => this.DisplayName);
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
