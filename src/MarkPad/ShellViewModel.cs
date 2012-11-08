using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using MarkPad.Document;
using MarkPad.Document.Events;
using MarkPad.Document.Search;
using MarkPad.DocumentSources;
using MarkPad.DocumentSources.FileSystem;
using MarkPad.DocumentSources.MetaWeblog;
using MarkPad.Events;
using MarkPad.Infrastructure;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Plugins;
using MarkPad.PreviewControl;
using MarkPad.Settings.UI;
using MarkPad.Updater;

namespace MarkPad
{
    internal class ShellViewModel : Conductor<IScreen>, IShell, IHandle<FileOpenEvent>, IHandle<SettingsCloseEvent>, IHandle<OpenFromWebEvent>
    {
        const string ShowSettingsState = "ShowSettings";
        readonly IEventAggregator eventAggregator;
        readonly IDialogService dialogService;
        readonly Func<DocumentViewModel> documentViewModelFactory;
        readonly List<string> loadingMessages = new List<string>();
        readonly IFileSystem fileSystem;

        public ShellViewModel(
            IDialogService dialogService,
            IEventAggregator eventAggregator,
            MdiViewModel mdi,
            SettingsViewModel settingsViewModel,
            UpdaterViewModel updaterViewModel,
            Func<DocumentViewModel> documentViewModelFactory, 
            IDocumentFactory documentFactory,
            IFileSystem fileSystem,
            SearchSettings searchSettings)
        {
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            MDI = mdi;
            Updater = updaterViewModel;
            this.documentViewModelFactory = documentViewModelFactory;
            this.documentFactory = documentFactory;
            this.fileSystem = fileSystem;
            SearchSettings = searchSettings;

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
        readonly IDocumentFactory documentFactory;
        readonly object workLock = new object();

        public string CurrentState
        {
            get { return currentState; }
            set
            {
                currentState = value;

                if (ActiveDocumentViewModel == null) return;
                MDI.HtmlPreview.Visibility = (currentState == ShowSettingsState) ? Visibility.Hidden : Visibility.Visible;
            }
        }

        public MdiViewModel MDI { get; private set; }
        public SettingsViewModel Settings { get; private set; }
        public UpdaterViewModel Updater { get; set; }
		public DocumentViewModel ActiveDocumentViewModel { get { return MDI.ActiveItem as DocumentViewModel; } }

        public string WorkingText { get; private set; }
        public bool IsWorking { get; private set; }

        public IDisposable DoingWork(string work)
        {
            lock (workLock)
            {
                IsWorking = true;
                loadingMessages.Add(work);
                WorkingText = work;

                return new DelegateDisposable(() =>
                {
                    lock (workLock)
                    {
                        loadingMessages.Remove(work);
                        IsWorking = loadingMessages.Count > 0;
                        if (loadingMessages.Count > 0)
                            WorkingText = loadingMessages.Last();
                    }
                });
            }
        }

        public void Exit()
        {
            TryClose();
        }

		public void NewDocument(string text = "")
		{
            // C.M passes in "text"...?
			if (text == "text") text = "";

			var documentViewModel = documentViewModelFactory();
            documentViewModel.Open(documentFactory.NewDocument(text));
			MDI.Open(documentViewModel);			
			documentViewModel.Update();
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
                var finishedLoading = DoingWork(string.Format("Saving {0}", doc.MarkpadDocument.Title));
                doc.Save()
                    .ContinueWith(t=>finishedLoading.Dispose(), TaskScheduler.FromCurrentSynchronizationContext());
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
            MDI.HtmlPreview.Print();
        }

        public void ShowHelp()
        {
            var documentViewModel = documentViewModelFactory();
            documentViewModel.Open(documentFactory.CreateHelpDocument("Markdown Help", GetHelpText("MarkdownHelp")));
            MDI.Open(documentViewModel);

            documentViewModel = documentViewModelFactory();
            documentViewModel.Open(documentFactory.CreateHelpDocument("MarkPad Help", GetHelpText("MarkPadHelp")));
            MDI.Open(documentViewModel);
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
            var doc = MDI.ActiveItem as DocumentViewModel;

            if (doc != null)
            {
                var finishedWork = DoingWork(string.Format("Publishing {0}", doc.MarkpadDocument.Title));
                doc.Publish()
                    .ContinueWith(t => finishedWork.Dispose(), TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public void OpenFromWeb()
        {
            documentFactory.OpenFromWeb()
                .ContinueWith(OpenDocumentResult, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void Handle(SettingsCloseEvent message)
        {
            CurrentState = "HideSettings";
        }

        public void Handle(FileOpenEvent message)
        {
            if (!fileSystem.File.Exists(message.Path)) return;

            var openedDocs = MDI.Items.Cast<DocumentViewModel>();
            var fileSystemSiteItem = new FileSystemSiteItem(eventAggregator, fileSystem, message.Path);
            var openedDoc = openedDocs.SingleOrDefault(d => d.MarkpadDocument.IsSameItem(fileSystemSiteItem));

            if (openedDoc != null)
                MDI.ActivateItem(openedDoc);
            else
            {
                var finishedLoading = DoingWork(string.Format("Opening {0}", message.Path));
                documentFactory
                    .OpenDocument(message.Path)
                    .ContinueWith(t =>
                    {
                        OpenDocumentResult(t);
                        finishedLoading.Dispose();
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public void Handle(OpenFromWebEvent message)
        {
            var finishedWork = DoingWork(string.Format("Opening {0}", message.Name));

            var openedDocs = MDI.Items.Cast<DocumentViewModel>();
            var metaWebLogItem = new WebDocumentItem(null, eventAggregator, message.Id, message.Name, message.Blog);
            var openedDoc = openedDocs.SingleOrDefault(d => d.MarkpadDocument.IsSameItem(metaWebLogItem));

            if (openedDoc != null)
                MDI.ActivateItem(openedDoc);
            else
            {
                documentFactory.OpenBlogPost(message.Blog, message.Id, message.Name)
                    .ContinueWith(t =>
                    {
                        OpenDocumentResult(t);
                        finishedWork.Dispose();
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        void OpenDocumentResult(Task<IMarkpadDocument> t)
        {
            if (t.IsCanceled || t.Result == null) return;
            var doc = documentViewModelFactory();
            doc.Open(t.Result);
            MDI.Open(doc);
        }

        public SearchSettings SearchSettings { get; private set; }

        public bool SearchingWithBar
        {
            get { return SearchSettings.SearchingWithBar; }
            set
            {
                if (SearchSettings.SearchingWithBar == value) return;

                SearchSettings.SearchingWithBar = value;
                if (!SearchSettings.SearchingWithBar)
                {
                    if (ActiveDocumentViewModel != null)
                    {
                        ActiveDocumentViewModel.View.Editor.Focus();
                    }
                    SearchSettings.SelectSearch = true;
                }
                if (SearchSettings.SearchingWithBar)
                {
                    SearchSettings.SelectSearch = false;
                    Search(SearchType.Normal);
                }
            }
        }

        public void Search(SearchType searchType)
        {
            if (ActiveDocumentViewModel == null) return;

            var selectSearch = SearchSettings.SelectSearch;
            if (searchType == SearchType.Next || searchType == SearchType.Prev)
            {
                selectSearch = true;
            }

            ActiveDocumentViewModel.SearchProvider.DoSearch(searchType, selectSearch);
            
            SearchSettings.SelectSearch = true;

            if (searchType == SearchType.Normal)
            {
                // update the search highlighting
                ActiveDocumentViewModel.View.Editor.TextArea.TextView.Redraw();
            }
        }
    }
}
