using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using Caliburn.Micro;
using CookComputing.XmlRpc;
using ICSharpCode.AvalonEdit.Document;
using MarkPad.Metaweblog;
using MarkPad.Services.Interfaces;
using MarkPad.Settings;
using Ookii.Dialogs.Wpf;

namespace MarkPad.Document
{
    internal class DocumentViewModel : Screen
    {
        private readonly IDialogService dialogService;
        private readonly ISettingsService _settings;

        private string title;
        private string filename;
        private readonly TimeSpan delay = TimeSpan.FromSeconds(0.5);
        private readonly DispatcherTimer timer;
        private Post _post;

        public DocumentViewModel(IDialogService dialogService, ISettingsService settings)
        {
            this.dialogService = dialogService;
            _settings = settings;

            title = "New Document";
            Original = "";
            Document = new TextDocument();
            _post = new Post();
            timer = new DispatcherTimer();
            timer.Tick += TimerTick;
            timer.Interval = delay;
        }
        private void TimerTick(object sender, EventArgs e)
        {
            timer.Stop();
            NotifyOfPropertyChange(() => Render);
        }
        public void Open(string path)
        {
            filename = path;
            title = new FileInfo(path).Name;

            var text = File.ReadAllText(path);
            Document.Text = text;
            Original = text;
        }

        public void OpenFromWeb(Post post)
        {
            _post = post;
            title = post.permalink;
            Document.Text = post.description;
            Original = post.description;
        }

        public Post Post { get { return _post; } }

        public void Update()
        {
            timer.Stop();
            timer.Start();
            NotifyOfPropertyChange(() => HasChanges);
            NotifyOfPropertyChange(() => DisplayName);
        }

        public bool Save()
        {
            if (!HasChanges)
                return true;

            if (string.IsNullOrEmpty(filename))
            {
                var path = dialogService.GetFileSavePath("Choose a location to save the document.", "*.md", Constants.ExtensionFilter + "|All Files (*.*)|*.*");

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
            get { return DocumentParser.Parse(Document.Text); }
        }

        public string RenderBody
        {
            get { return DocumentParser.GetBodyContents(Document.Text); }
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

        public void Print()
        {
            var view = this.GetView() as DocumentView;
            if (view != null)
            {
                view.wb.Print();
            }
        }

        public void Publish(string postTitle, string[] categories, BlogSetting blog)
        {
            if (categories == null) categories = new string[0];

            var proxy = XmlRpcProxyGen.Create<IMetaWeblog>();
            ((IXmlRpcProxy) proxy).Url = blog.WebAPI;

            var post = new Post();

            var permalink = this.DisplayName.Split('.')[0] == "New Document"
                                ? postTitle
                                : this.DisplayName.Split('.')[0];

            if (!string.IsNullOrWhiteSpace(_post.postid.ToString()))
            {
                post = proxy.GetPost(_post.postid.ToString(), blog.Username, blog.Password);
            }

            try
            {
                if (string.IsNullOrWhiteSpace(post.permalink))
                {
                    post = new Post
                               {
                                   permalink = permalink,
                                   title = postTitle,
                                   dateCreated = DateTime.Now,
                                   description = blog.Language == "HTML" ? RenderBody : Document.Text,
                                   categories = categories
                               };
                    post.postid = proxy.AddPost(blog.BlogInfo.blogid, blog.Username, blog.Password, post, true);

                    _settings.Set(post.permalink, post);
                    _settings.Save();
                }
                else
                {
                    post.title = postTitle;
                    post.description = blog.Language == "HTML" ? RenderBody : Document.Text;
                    post.categories = categories;

                    proxy.UpdatePost(post.postid.ToString(), blog.Username, blog.Password, post, true);

                    _settings.Set(post.permalink, post);
                    _settings.Save();
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Message, "Error Publishing", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _post = post;
            Original = Document.Text;
            title = postTitle;
            NotifyOfPropertyChange(() => DisplayName);
        }
    }
}
