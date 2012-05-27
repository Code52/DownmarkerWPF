using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using CookComputing.XmlRpc;
using ICSharpCode.AvalonEdit.Document;
using MarkPad.HyperlinkEditor;
using MarkPad.Services;
using MarkPad.Services.Implementation;
using MarkPad.Services.Interfaces;
using MarkPad.Services.Metaweblog;
using MarkPad.Services.Settings;
using Ookii.Dialogs.Wpf;

namespace MarkPad.Document
{
    public class DocumentViewModel : Screen
    {
        private static readonly ILog Log = LogManager.GetLog(typeof(DocumentViewModel));
        private const double ZoomDelta = 0.1;
        private double zoomLevel = 1;

        private readonly IDialogService dialogService;
        private readonly IWindowManager windowManager;
        private readonly ISiteContextGenerator siteContextGenerator;
        private readonly Func<string, IMetaWeblogService> getMetaWeblog;
        private readonly ISettingsProvider settingsProvider;

        private readonly TimeSpan delay = TimeSpan.FromSeconds(0.5);
        private readonly DispatcherTimer timer;

        private string title;

        readonly Regex wordCountRegex = new Regex(@"[\S]+", RegexOptions.Compiled);

        public DocumentViewModel(
            IDialogService dialogService, 
            IWindowManager windowManager, 
            ISiteContextGenerator siteContextGenerator,
            Func<string, IMetaWeblogService> getMetaWeblog,
            ISettingsProvider settingsProvider)
        {
            this.dialogService = dialogService;
            this.windowManager = windowManager;
            this.siteContextGenerator = siteContextGenerator;
            this.getMetaWeblog = getMetaWeblog;
            this.settingsProvider = settingsProvider;

            FontSize = GetFontSize();

            title = "New Document";
            Original = "";
            Document = new TextDocument();
            Post = new Post();
            timer = new DispatcherTimer();
            timer.Tick += TimerTick;
            timer.Interval = delay;
        }

        private void UpdateWordCount()
        {
            WordCount = wordCountRegex.Matches(Document.Text).Count;
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
                if (SiteContext != null)
                {
                    result = SiteContext.ConvertToAbsolutePaths(result);
                }

                Render = result;
                UpdateWordCount();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void Open(string path)
        {
            FileName = path;
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

            title = post.title ?? string.Empty;
            Document.Text = post.description ?? string.Empty;
            Original = post.description ?? string.Empty;

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

        public bool SaveAs()
        {
            var path = dialogService.GetFileSavePath("Save As", "*.md", Constants.ExtensionFilter + "|All Files (*.*)|*.*");

            if (string.IsNullOrEmpty(path))
                return false;

            FileName = path;

            if (!Save())
                return false;

            title = new FileInfo(FileName).Name;
            NotifyOfPropertyChange(() => DisplayName);
            EvaluateContext();

            return true;
        }

        public bool Save()
        {
            if (string.IsNullOrEmpty(FileName))
                return SaveAs();

            try
            {
                File.WriteAllText(FileName, Document.Text);
            }
            catch (Exception)
            {
                var saveResult = dialogService.ShowConfirmation("MarkPad", "Cannot save file",
                                                "You may not have permission to save this file to the selected location, or the location may not be currently available.",
                                                new ButtonExtras(ButtonType.Yes, "Select a different location", "Save this file to a different location."),
                                                new ButtonExtras(ButtonType.No, "Cancel", ""));

                if (!saveResult)
                    return false;

                return SaveAs();
            }

            Original = Document.Text;
            return true;
        }

        private void EvaluateContext()
        {
            SiteContext = siteContextGenerator.GetContext(FileName);
        }

        public TextDocument Document { get; private set; }

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

        public string FileName { get; private set; }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public bool FloatingToolbarEnabled
        {
            get
            {
                var settings = settingsProvider.GetSettings<MarkPadSettings>();
                return settings.FloatingToolBarEnabled;
            }
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

            var saveResult = dialogService.ShowConfirmationWithCancel("MarkPad", "Save file", "Do you want to save the changes to '" + title + "'?",
                new ButtonExtras(ButtonType.Yes, "Save",
                    string.IsNullOrEmpty(FileName) ? "The file has not been saved yet" : "The file will be saved to " + Path.GetFullPath(FileName)),
                new ButtonExtras(ButtonType.No, "Discard", "Discard the changes to this file"),
                new ButtonExtras(ButtonType.Cancel, "Cancel", "Cancel closing the application")
            );

            if (saveResult == true)
            {
                var saved = Save();
                if (saved) CheckAndCloseView(view);
                callback(saved);
                return;
            }

            if (saveResult == false)
            {
                CheckAndCloseView(view);
                callback(true);
                return;
            }

            callback(false);
        }

        private static void CheckAndCloseView(DocumentView view)
        {
            if (view != null)
            {
                view.Cleanup();
            }
        }

        public void Print()
        {
            var view = GetView() as DocumentView;
            if (view != null)
            {
                view.Print();
            }
        }

        public bool DistractionFree { get; set; }

        public ISiteContext SiteContext { get; private set; }

        public int WordCount { get; private set; }

        public double FontSize { get; private set; }

        public double ZoomLevel
        {
            get { return zoomLevel; }
            set
            {
                zoomLevel = value;
                FontSize = GetFontSize()*value;
            }
        }

        public double MaxZoom
        {
            get { return 2; }
        }

        public double MinZoom
        {
            get { return 0.5; }
        }

        /// <summary>
        /// Get the font size that was set in the settings.
        /// </summary>
        /// <returns>Font size.</returns>
        private int GetFontSize()
        {
            return Constants.FONT_SIZE_ENUM_ADJUSTMENT + (int)settingsProvider.GetSettings<MarkPadSettings>().FontSize;
        }

        public void ZoomIn()
        {
            AdjustZoom(ZoomDelta);
        }

        public void ZoomOut()
        {
            AdjustZoom(-ZoomDelta);
        }

        private void AdjustZoom(double delta)
        {
            var newZoom = ZoomLevel + delta;

            if (newZoom < MinZoom)
            {
                //Don't cause a change if we don't have to
                if (Math.Abs(ZoomLevel - MinZoom) < 0.1) return;
                newZoom = MinZoom;
            }
            if (newZoom > MaxZoom)
            {
                //Don't cause a change if we don't have to
                if (Math.Abs(ZoomLevel - MaxZoom) < 0.1)return;
                newZoom = MaxZoom;
            }

            ZoomLevel = newZoom;
        }

        public void ZoomReset()
        {
            ZoomLevel = 1;
        }

        public void Publish(string postid, string postTitle, string[] categories, BlogSetting blog)
        {
            if (categories == null) categories = new string[0];

            var proxy = getMetaWeblog(blog.WebAPI);

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
                                   categories = categories,
                                   format = blog.Language
                               };
                    newpost.postid = proxy.NewPost(blog, newpost, true);
                }
                else
                {
                    newpost = proxy.GetPost(postid, blog);
                    newpost.title = postTitle;
                    newpost.description = blog.Language == "HTML" ? renderBody : Document.Text;
                    newpost.categories = categories;
                    newpost.format = blog.Language;

                    proxy.EditPost(postid, blog, newpost, true);
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

        public void RefreshFont()
        {
            FontSize = GetFontSize()*ZoomLevel;
        }
    }
}
