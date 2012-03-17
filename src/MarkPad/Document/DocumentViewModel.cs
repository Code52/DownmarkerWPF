using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using Caliburn.Micro;
using CookComputing.XmlRpc;
using ICSharpCode.AvalonEdit.Document;
using MarkPad.Framework;
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
		private readonly ISiteContextGenerator siteContextGenerator;

        private readonly TimeSpan delay = TimeSpan.FromSeconds(0.5);
        private readonly DispatcherTimer timer;

        private string title;
        private string filename;
        private ISiteContext siteContext;

        public DocumentViewModel(IDialogService dialogService, ISettingsService settings, IWindowManager windowManager, ISiteContextGenerator siteContextGenerator)
        {
            this.dialogService = dialogService;
            this.settings = settings;
            this.windowManager = windowManager;
            this.siteContextGenerator = siteContextGenerator;

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

            Task.Factory.StartNew(text => DocumentParser.Parse(text.ToString()), Document.Text)
            .ContinueWith(s =>
            {
                if (s.IsFaulted)
                {
                    Log.Error(s.Exception);
                    return;
                }

                var result = s.Result;
                if (siteContext != null)
                {
                    result = siteContext.ConvertToAbsolutePaths(result);
                }

                Render = result;
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
            EvaluateContext();
        }

        public void OpenFromWeb(Post post)
        {
            Post = post;
            title = post.permalink;
            Document.Text = post.description;
            Original = post.description;

            Update();
            EvaluateContext();
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
                EvaluateContext();
            }

            try
            {
                File.WriteAllText(filename, Document.Text);
                Original = Document.Text;
            }
            catch(Exception)
            {
                var saveResult = dialogService.ShowConfirmation("MarkPad", "Cannot save file",
                                                String.Format("Do you want to save changes for {0} to a different file?", title), 
                                                new ButtonExtras(ButtonType.Yes, "Save", "Save the file at a different location."),
                                                new ButtonExtras(ButtonType.No, "Do not save", "The file will be considered a New Document.  The next save will prompt for a file location."));

                string prevFileName = filename;
                string prevTitle = title;

                title = "New Document";
                filename = "";
                if (saveResult)
                {
                    saveResult = Save();
                    if (!saveResult)  //We decide not to save, keep existing title and filename 
                    {
                        title = prevTitle;
                        filename = prevFileName;
                    }
                }

                NotifyOfPropertyChange(() => DisplayName);
                return saveResult;
            }
             
            return true;
        }

        private void EvaluateContext()
        {
            siteContext = siteContextGenerator.GetContext(filename);
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

        public string FileName
        {
            get { return filename; } 
        }

        public override void CanClose(Action<bool> callback)
        {
            var view = GetView() as DocumentView;

            if (!HasChanges)
            {
                CheckAndCloseView(view);
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
            if (result)
            {
                CheckAndCloseView(view);
            }

            callback(result);
        }

        private static void CheckAndCloseView(DocumentView view)
        {
            if (view != null
                && view.wb != null)
            {
                view.wb.Close();
            }
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

        public ISiteContext SiteContext
        {
            get { return siteContext; }
        }

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
            var viewModel = new HyperlinkEditorViewModel(hyperlink.Text, hyperlink.Url)
                                {
                                    IsUrlFocussed = !String.IsNullOrWhiteSpace(hyperlink.Text)
                                };
            windowManager.ShowDialog(viewModel);
            if (!viewModel.WasCancelled)
            {
                hyperlink.Set(viewModel.Text, viewModel.Url);
                return hyperlink;
            }
            return null;
        }

        public int GetFontSize()
        {
            return (int) settings.Get<FontSizes>(SettingsViewModel.FontSizeSettingsKey);
        }
		public FontFamily GetFontFamily()
		{
			var configuredSource = settings.Get<string>(SettingsViewModel.FontFamilySettingsKey);
			var fontFamily = FontHelpers.TryGetFontFamilyFromStack(configuredSource, "Segoe UI", "Arial");
			if (fontFamily == null) throw new Exception("Cannot find configured font family or fallback fonts");
			return fontFamily;			
		}
    }
}
