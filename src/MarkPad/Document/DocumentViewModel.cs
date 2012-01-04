using System;
using System.IO;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit.Document;
using MarkPad.Metaweblog;
using MarkdownSharp;
using MarkPad.Services.Interfaces;
using CookComputing.XmlRpc;

namespace MarkPad.Document
{
    internal class DocumentViewModel : Screen
    {
        private readonly IDialogService dialogService;
        private readonly ISettingsService _settings;

        private string title;
        private string filename;

        public DocumentViewModel(IDialogService dialogService, ISettingsService settings)
        {
            this.dialogService = dialogService;
            _settings = settings;

            title = "New Document";
            Original = "";
            Document = new TextDocument();
        }

        public void Open(string filename)
        {
            this.filename = filename;
            title = new FileInfo(filename).Name;

            var text = File.ReadAllText(filename);
            Document.Text = text;
            Original = text;
        }

        public void Update()
        {
            NotifyOfPropertyChange(() => Render);
            NotifyOfPropertyChange(() => HasChanges);
            NotifyOfPropertyChange(() => DisplayName);
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
            Original = Document.Text;
        }

        public TextDocument Document { get; set; }
        public string Original { get; set; }

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
            get { return Original != Document.Text; }
        }

        public override string DisplayName
        {
            get { return title + (HasChanges ? " *" : ""); }
        }

        public void Publish()
        {
            var proxy = XmlRpcProxyGen.Create<IMetaWeblog>();
            ((IXmlRpcProxy) proxy).Url = _settings.Get<string>("BlogUrl");

            var post = new Post
                           {
                               permalink = "testing1123",
                               title = "testing this post",
                               dateCreated = DateTime.Now,
                               description = Document.Text,
                               categories = new string[0],
                           };
            proxy.AddPost("0", _settings.Get<string>("Username"), _settings.Get<string>("Password"), post, true);
        }
    }
}
