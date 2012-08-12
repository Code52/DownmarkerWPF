using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using MarkPad.Document;
using MarkPad.DocumentSources.MetaWeblog;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Events;
using MarkPad.Infrastructure.DialogService;
using MarkPad.PreviewControl;
using MarkPad.Settings;
using MarkPad.Settings.Models;
using MarkPad.Settings.UI;
using Ookii.Dialogs.Wpf;
using MarkPad.Updater;

namespace MarkPad
{
    internal class ShellViewModel : Conductor<IScreen>, IHandle<FileOpenEvent>, IHandle<OpenFromWebEvent>, IHandle<SettingsCloseEvent>
    {
        private const string ShowSettingsState = "ShowSettings";
        private readonly IEventAggregator eventAggregator;
        private readonly IDialogService dialogService;
        private readonly IWindowManager windowManager;
        private readonly ISettingsProvider settingsService;
        private readonly Func<DocumentViewModel> documentCreator;
        private readonly Func<OpenFromWebViewModel> openFromWebCreator;

        public ShellViewModel(
            IDialogService dialogService,
            IWindowManager windowManager,
            ISettingsProvider settingsService,
            IEventAggregator eventAggregator,
            MdiViewModel mdi,
            SettingsViewModel settingsViewModel,
            UpdaterViewModel updaterViewModel,
            Func<DocumentViewModel> documentCreator,
            Func<OpenFromWebViewModel> openFromWebCreator)
        {
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            this.windowManager = windowManager;
            this.settingsService = settingsService;
            MDI = mdi;
            Updater = updaterViewModel;
            this.documentCreator = documentCreator;
            this.openFromWebCreator = openFromWebCreator;

            Settings = settingsViewModel;
            Settings.Initialize();

            ActivateItem(mdi);
        }

        public override string DisplayName
        {
            get { return "MarkPad"; }
            set { }
        }

        private string currentState;
        public string CurrentState
        {
            get { return currentState; }
            set
            {
                currentState = value;

                if (ActiveDocumentViewModel == null) return;
                var shellView = (ShellView)GetView();
                ((ShellViewModel)shellView.DataContext).MDI.HtmlPreview.Visibility = currentState == ShowSettingsState
                                                          ? Visibility.Hidden
                                                          : Visibility.Visible;
            }
        }

        public MdiViewModel MDI { get; private set; }
        public SettingsViewModel Settings { get; private set; }
        public UpdaterViewModel Updater { get; set; }
		public DocumentViewModel ActiveDocumentViewModel { get { return MDI.ActiveItem as DocumentViewModel; } }

        public void Exit()
        {
            TryClose();
        }

		public void NewDocument(string text = "")
		{
            // C.M passes in "text"...?
			if (text == "text") text = "";

			var creator = documentCreator();

            creator.Document.BeginUpdate();
			creator.Document.Text = text;
            creator.Document.EndUpdate();
            
			MDI.Open(creator);			
			creator.Update();
		}

        public void NewJekyllDocument()
        {
			NewDocument(CreateJekyllHeader());
        }

        private static string CreateJekyllHeader()
        {
            const string permalink = "new-page.html";
            const string title = "New Post";
            const string description = "Some Description";
            var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss zzz");
            return string.Format("---\r\nlayout: post\r\ntitle: {0}\r\npermalink: {1}\r\ndescription: {2}\r\ndate: {3}\r\ntags: \"some tags here\"\r\n---\r\n\r\n", title, permalink, description, date);
        }

        public void OpenDocument()
        {
            var path = dialogService.GetFileOpenPath("Open a markdown document.", Constants.ExtensionFilter + "|Any File (*.*)|*.*");
            if (path == null)
                return;

            OpenDocument(path);
        }

        public void OpenDocument(IEnumerable<string> filenames)
        {
            if (filenames == null) return;

            foreach (var fn in filenames)
            {
                eventAggregator.Publish(new FileOpenEvent(fn));
            }
        }

        public void SaveDocument()
        {
            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                doc.Save();
            }
        }

        public void SaveAsDocument()
        {
            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                doc.SaveAs();
            }
        }

        public void SaveAllDocuments()
        {
            foreach (DocumentViewModel doc in MDI.Items)
            {
                doc.Save();
            }
        }

        public void CloseDocument()
        {
            if (CurrentState == ShowSettingsState)
            {
                Handle(new SettingsCloseEvent());
                return;
            }

            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                MDI.CloseItem(doc);
                return;
            }

            TryClose();
        }

        public void Handle(FileOpenEvent message)
        {
            DocumentViewModel openedDoc = GetOpenedDocument(message.Path);

            if (openedDoc != null)
                MDI.ActivateItem(openedDoc);
            else
            {
                if (File.Exists(message.Path))
                {
                    var doc = documentCreator();
                    doc.Open(message.Path);
                    MDI.Open(doc);
                }
            }
        }

        public void ShowSettings()
        {
            CurrentState = ShowSettingsState;
        }

        public void ToggleWebView()
        {
            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                doc.DistractionFree = !doc.DistractionFree;
            }
        }

        public void PrintDocument()
        {
            var shellView = (ShellViewModel)GetView();
            shellView.MDI.HtmlPreview.Print();
        }

        /// <summary>
        /// Returns opened document with a given filename. 
        /// </summary>
        /// <param name="filename">Fully qualified path to the document file.</param>
        /// <returns>Opened document or null if file hasn't been yet opened.</returns>
        private DocumentViewModel GetOpenedDocument(string filename)
        {
            if (filename == null)
                return null;

            var openedDocs = MDI.Items.Cast<DocumentViewModel>();

            return openedDocs.FirstOrDefault(doc => doc != null && filename.Equals(doc.FileName));
        }

        public void ShowHelp()
        {
            var creator = documentCreator();
            creator.Original = GetHelpText("MarkdownHelp"); // set the Original so it isn't marked as requiring a save unless we change it
            creator.Document.Text = creator.Original;
            creator.Title = "Markdown Help";
            MDI.Open(creator);
            creator.Update(); // ensure that the markdown is rendered

            creator = documentCreator();
            creator.Original = GetHelpText("MarkPadHelp"); // set the Original so it isn't marked as requiring a save unless we change it
            creator.Document.Text = creator.Original;
            creator.Title = "MarkPad Help";
            MDI.Open(creator);
            creator.Update(); // ensure that the markdown is rendered
        }

        private static string GetHelpText(string file)
        {
            var helpResourceFile = "MarkPad." + file + ".md";
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(helpResourceFile))
            using (var streamReader = new StreamReader(resourceStream))
            {
                return streamReader.ReadToEnd();
            }
        }

        public void PublishDocument()
        {
            var settings = settingsService.GetSettings<MarkPadSettings>();
            var blogs = settings.GetBlogs();
            if (blogs == null || blogs.Count == 0)
            {
                if (!ConfigureNewBlog("Publish document"))
                    return;
                blogs = settings.GetBlogs();
                if (blogs == null || blogs.Count == 0)
                    return;
            }

            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                var post = doc.Post ?? new Post();
                var pd = new Details { Title = post.title, Categories = post.categories };
                var detailsResult = windowManager.ShowDialog(new PublishDetailsViewModel(pd, blogs));
                if (detailsResult != true)
                    return;

                doc.Publish(post.postid == null ? null : post.postid.ToString(), pd.Title, pd.Categories, pd.Blog);
            }
        }

        public void OpenFromWeb()
        {
            var settings = settingsService.GetSettings<MarkPadSettings>();
            var blogs = settings.GetBlogs();
            if (blogs == null || blogs.Count == 0)
            {
                if (!ConfigureNewBlog("Open from web"))
                    return;
                blogs = settings.GetBlogs();
                if (blogs == null || blogs.Count == 0)
                    return;
            }

            var openFromWeb = openFromWebCreator();
            openFromWeb.InitializeBlogs(blogs);

            var result = windowManager.ShowDialog(openFromWeb);
            if (result != true)
                return;

            var post = openFromWeb.SelectedPost;

            var doc = documentCreator();
            doc.OpenFromWeb(openFromWeb.SelectedBlog, post);
            MDI.Open(doc);
        }

        bool ConfigureNewBlog(string featureName)
        {
            var extra = string.Format(
                "The '{0}' feature requires a blog to be configured. A window will be displayed which will allow you to configure a blog.",
                featureName);
            var setupBlog = dialogService.ShowConfirmation(
                "No blogs are configured",
                "Do you want to configure a blog?",
                extra,
                new ButtonExtras(ButtonType.Yes, "Yes", "Configure a blog"),
                new ButtonExtras(ButtonType.No, "No", "Don't configure a blog"));

            if (!setupBlog)
                return false;
            if (!Settings.AddBlog())
                return false;

            Settings.Accept();

            return true;
        }

        public void Handle(SettingsCloseEvent message)
        {
            CurrentState = "HideSettings";
        }

        public void Handle(OpenFromWebEvent message)
        {
            var doc = documentCreator();
            doc.OpenFromWeb(message.SelectedBlog, message.Post);
            MDI.Open(doc);
        }
    }
}
