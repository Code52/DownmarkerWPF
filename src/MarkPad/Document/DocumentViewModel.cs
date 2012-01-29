using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using CookComputing.XmlRpc;
using ICSharpCode.AvalonEdit.Document;
using MarkPad.HyperlinkEditor;
using MarkPad.Metaweblog;
using MarkPad.Services.Interfaces;
using MarkPad.Settings;
using Ookii.Dialogs.Wpf;

namespace MarkPad.Document
{
    internal class DocumentViewModel : Screen
    {
        private static readonly ILog Log = LogManager.GetLog(typeof(DocumentViewModel));

        private readonly IDialogService dialogService;
        private readonly ISettingsService settings;
        private readonly IWindowManager windowManager;

        private readonly TimeSpan delay = TimeSpan.FromSeconds(0.5);
        private readonly DispatcherTimer timer;

        private string title;
        private string filename;

        public DocumentViewModel(IDialogService dialogService, ISettingsService settings, IWindowManager windowManager)
        {
            this.dialogService = dialogService;
            this.settings = settings;
            this.windowManager = windowManager;

            title = "New Document";
            Original = "";
            Document = new TextDocument();
            Post = new Post();
            timer = new DispatcherTimer();
            timer.Tick += TimerTick;
            timer.Interval = delay;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            timer.Stop();

            Task.Factory.StartNew<string>(text =>
            {
                return DocumentParser.Parse(text.ToString());
            }, Document.Text)
            .ContinueWith(s =>
            {
                if (s.IsFaulted)
                {
                    Log.Error(s.Exception);
                    return;
                }

                this.Render = s.Result;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void Open(string path)
        {
            filename = path;
            title = new FileInfo(path).Name;

            var text = File.ReadAllText(path);
            Document.Text = text;
            Original = text;

            Update();
        }

        public void OpenFromWeb(Post post)
        {
            Post = post;
            title = post.permalink;
            Document.Text = post.description;
            Original = post.description;

            Update();
        }

        public Post Post { get; private set; }

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

        public string Render { get; private set; }

        public bool HasChanges
        {
            get { return Original != Document.Text; }
        }

        public override string DisplayName
        {
            get { return title; }
        }

        public override void CanClose(Action<bool> callback)
        {
            DocumentView view = (DocumentView)this.GetView();

            if (!HasChanges)
            {
                view.wb.Close();
                callback(true);
                return;
            }

            var saveResult = dialogService.ShowConfirmationWithCancel("MarkPad", "Save modifications.", "Do you want to save your changes to '" + title + "'?",
                new ButtonExtras(ButtonType.Yes, "Save",
                    string.IsNullOrEmpty(filename) ? "The file has not been saved yet" : "The file will be saved to " + Path.GetFullPath(filename)),
                new ButtonExtras(ButtonType.No, "Don't Save", "Close the document without saving the modifications"),
                new ButtonExtras(ButtonType.Cancel, "Cancel", "Don't close the document")
            );
            var result = false;

            // true = Yes
            switch (saveResult)
            {
                case true:
                    result = Save();
                    break;
                case false:
                    result = true;
                    break;
            }

            // Close browser if tab is being closed
            if (result == true)
            {
                view.wb.Close();
            }

            callback(result);
        }

        public void Print()
        {
            var view = GetView() as DocumentView;
            if (view != null)
            {
                view.wb.Print();
            }
        }

        public bool DistractionFree { get; set; }

        public void Publish(string postid, string postTitle, string[] categories, BlogSetting blog)
        {
            if (categories == null) categories = new string[0];

            var proxy = new MetaWeblog(blog.WebAPI);

            var newpost = new Post();
            try
            {
                var renderBody = DocumentParser.GetBodyContents(Document.Text);

                if (string.IsNullOrWhiteSpace(postid))
                {
                    var permalink = DisplayName.Split('.')[0] == "New Document"
                                ? postTitle
                                : DisplayName.Split('.')[0];

                    newpost = new Post
                               {
                                   permalink = permalink,
                                   title = postTitle,
                                   dateCreated = DateTime.Now,
                                   description = blog.Language == "HTML" ? renderBody : Document.Text,
                                   categories = categories
                               };
                    newpost.postid = proxy.NewPost(blog.BlogInfo.blogid, blog.Username, blog.Password, newpost, true);

                    settings.Set(newpost.permalink, newpost);
                    settings.Save();
                }
                else
                {
                    newpost = proxy.GetPost(postid, blog.Username, blog.Password);
                    newpost.title = postTitle;
                    newpost.description = blog.Language == "HTML" ? renderBody : Document.Text;
                    newpost.categories = categories;
                    newpost.format = blog.Language;

                    proxy.EditPost(postid, blog.Username, blog.Password, newpost, true);

                    //Not sure what this is doing??
                    settings.Set(newpost.permalink, newpost);
                    settings.Save();
                }
            }
            catch (WebException ex)
            {
                dialogService.ShowError("Error Publishing", ex.Message, "");
            }
            catch (XmlRpcException ex)
            {
                dialogService.ShowError("Error Publishing", ex.Message, "");
            }
            catch (XmlRpcFaultException ex)
            {
                dialogService.ShowError("Error Publishing", ex.Message, "");
            }

            Post = newpost;
            Original = Document.Text;
            title = postTitle;
            NotifyOfPropertyChange(() => DisplayName);
        }

        public MarkPadHyperlink GetHyperlink(MarkPadHyperlink hyperlink)
        {
            var viewModel = new HyperlinkEditorViewModel(hyperlink.Text, hyperlink.Url);
            windowManager.ShowDialog(viewModel);
            if (!viewModel.WasCancelled)
            {
                hyperlink.Set(viewModel.Text, viewModel.Url);
				return hyperlink;
            }
            return null;
        }
    }
}
