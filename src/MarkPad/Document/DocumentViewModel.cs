using System;
using System.IO;
using System.Text.RegularExpressions;
using Caliburn.Micro;
using CookComputing.XmlRpc;
using ICSharpCode.AvalonEdit.Document;
using MarkdownSharp;
using MarkPad.Metaweblog;
using MarkPad.Services.Interfaces;
using Ookii.Dialogs.Wpf;

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

        public void Open(string path)
        {
            filename = path;
            title = new FileInfo(path).Name;

            var text = File.ReadAllText(path);
            Document.Text = text;
            Original = text;
        }

        public void OpenFromWeb(string postTitle, string text)
        {
            title = postTitle;
            Document.Text = text;
            Original = text;
        }

        public void Update()
        {
            NotifyOfPropertyChange(() => Render);
            NotifyOfPropertyChange(() => HasChanges);
            NotifyOfPropertyChange(() => DisplayName);
        }

        public bool Save()
        {
            if (!HasChanges)
                return true;

            if (string.IsNullOrEmpty(filename))
            {
                var path = dialogService.GetFileSavePath("Choose a location to save the document.", "*.md", "Markdown Files (*.md)|*.md|All Files (*.*)|*.*");

                if (string.IsNullOrEmpty(path))
                    return false;

                filename = path;
                title = new FileInfo(filename).Name;
                NotifyOfPropertyChange(() => DisplayName);
            }

            File.WriteAllText(filename, Document.Text);
            Original = Document.Text;

            return true;
        }

        public TextDocument Document { get; set; }
        public string Original { get; set; }

        public string Render
        {
            get
            {
                var markdown = new Markdown();

                var textToRender = StripHeader(Document.Text);

                return markdown.Transform(textToRender);
            }
        }

        private static string StripHeader(string text)
        {
            const string delimiter = "---";
            var matches = Regex.Matches(text, delimiter, RegexOptions.Multiline);

            if (matches.Count != 2)
            {
                return text;
            }

            var startIndex = matches[0].Index;
            var endIndex = matches[1].Index;
            var length = endIndex - startIndex + delimiter.Length;
            var textToReplace = text.Substring(startIndex, length);

            return text.Replace(textToReplace, string.Empty);
        }

        public bool HasChanges
        {
            get { return Original != Document.Text; }
        }

        public override string DisplayName
        {
            get { return title; }
        }

        public override void CanClose(System.Action<bool> callback)
        {
            if (!HasChanges)
            {
                callback(true);
                return;
            }

            var saveResult = dialogService.ShowConfirmationWithCancel("MarkPad", "Save modifications.", "Do you want to save your changes to '" + title + "'?",
                new ButtonExtras(ButtonType.Yes, "Save",
                    string.IsNullOrEmpty(filename) ? "The file has not been saved yet" : "The file will be saved to " + Path.GetFullPath(filename)),
                new ButtonExtras(ButtonType.No, "Close", "Close the document without saving the modifications"),
                new ButtonExtras(ButtonType.Cancel, "Cancel", "Don't close the document")
            );
            bool result = false;

            // true = Yes
            if (saveResult == true)
            {
                result = Save();
            }
            // false = No
            else if (saveResult == false)
            {
                result = true;
            }
            // no result = Cancel
            else if (!saveResult.HasValue)
            {
                result = false;
            }

            callback(result);
        }

        public void Publish()
        {
            var proxy = XmlRpcProxyGen.Create<IMetaWeblog>();
            ((IXmlRpcProxy)proxy).Url = _settings.Get<string>("BlogUrl");

            var post = new Post();

            var postTitle = this.DisplayName.Split('.')[0];

            var savedPost = _settings.Get<Post>(postTitle);
            
            if (!string.IsNullOrWhiteSpace(savedPost.permalink))
            {
                post = proxy.GetPost(savedPost.postid.ToString(), _settings.Get<string>("Username"), _settings.Get<string>("Password"));
            }

            if (string.IsNullOrWhiteSpace(post.permalink))
            {
                post = new Post
                           {
                               permalink = postTitle,
                               title = "Teeeeeeest",
                               dateCreated = DateTime.Now,
                               description = Render,
                               categories = new string[0]
                           };
              post.postid = proxy.AddPost("0", _settings.Get<string>("Username"), _settings.Get<string>("Password"), post, true);

                _settings.Set(post.permalink, post);
                _settings.Save();
            }
            else
            {

                post.description = Render;

                proxy.UpdatePost(post.postid.ToString(), _settings.Get<string>("Username"),
                                          _settings.Get<string>("Password"), post, true);

                _settings.Set(post.permalink, post);
                _settings.Save();
            }

            
        }
    }
}
