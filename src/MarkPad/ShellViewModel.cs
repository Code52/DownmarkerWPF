using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
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
    public class ShellViewModel : Conductor<IScreen>, IShell, IHandle<FileOpenEvent>, IHandle<SettingsCloseEvent>, IHandle<OpenFromWebEvent>
    {
        const string ShowSettingsState = "ShowSettings";
        const string NewDocumentDefaultName = "New Document";
        readonly IEventAggregator eventAggregator;
        readonly IDialogService dialogService;
        readonly IOpenDocumentFromWeb openDocumentFromWeb;
        readonly Func<DocumentViewModel> documentViewModelFactory;
        readonly List<string> loadingMessages = new List<string>();
        readonly IFileSystem fileSystem;
        readonly IDocumentFactory documentFactory;
        readonly object workLock = new object();

        private int numberOfNewDocuments;

        public ShellViewModel(
            IDialogService dialogService,
            IEventAggregator eventAggregator,
            MdiViewModel mdi,
            SettingsViewModel settingsViewModel,
            UpdaterViewModel updaterViewModel,
            Func<DocumentViewModel> documentViewModelFactory,
            IDocumentFactory documentFactory,
            IFileSystem fileSystem,
            SearchSettings searchSettings, IOpenDocumentFromWeb openDocumentFromWeb)
        {
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            MDI = mdi;
            Updater = updaterViewModel;
            this.documentViewModelFactory = documentViewModelFactory;
            this.documentFactory = documentFactory;
            this.fileSystem = fileSystem;
            SearchSettings = searchSettings;
            this.openDocumentFromWeb = openDocumentFromWeb;

            Settings = settingsViewModel;
            Settings.Initialize();
            NewDocumentCommand = new DelegateCommand(() => NewDocument());
            NewJekyllDocumentCommand = new DelegateCommand(() => NewDocument(CreateJekyllHeader()));
            SaveDocumentCommand = new AwaitableDelegateCommand(SaveDocument, () => !IsWorking);
            SaveDocumentAsCommand = new AwaitableDelegateCommand(SaveDocumentAs, () => !IsWorking);
            SaveAllDocumentsCommand = new AwaitableDelegateCommand(SaveAllDocuments, () => !IsWorking);
            PublishDocumentCommand = new AwaitableDelegateCommand(PublishDocument, () => !IsWorking);
            OpenDocumentCommand = new DelegateCommand(OpenDocument, () => !IsWorking);
            OpenFromWebCommand = new AwaitableDelegateCommand(OpenFromWeb, () => !IsWorking);
            CloseDocumentCommand = new DelegateCommand(CloseDocument, () => ActiveDocumentViewModel != null);

            ActivateItem(mdi);
        }

        public override string DisplayName
        {
            get { return "MarkPad"; }
            set { }
        }

        public ICommand NewDocumentCommand { get; private set; }
        public ICommand NewJekyllDocumentCommand { get; private set; }
        public IAsyncCommand SaveDocumentCommand { get; private set; }
        public IAsyncCommand SaveDocumentAsCommand { get; private set; }
        public IAsyncCommand SaveAllDocumentsCommand { get; private set; }
        public IAsyncCommand PublishDocumentCommand { get; private set; }
        public ICommand OpenDocumentCommand { get; private set; }
        public ICommand OpenFromWebCommand { get; private set; }
        public ICommand CloseDocumentCommand { get; private set; }

        private string currentState;

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
                var view = (DependencyObject)GetView();
                AutomationProperties.SetHelpText(view, "Busy");

                return new DelegateDisposable(() =>
                {
                    lock (workLock)
                    {
                        loadingMessages.Remove(work);
                        if (loadingMessages.Count > 0)
                        {
                            IsWorking = true;
                            WorkingText = loadingMessages.Last();
                        }
                        else
                        {
                            IsWorking = false;
                            WorkingText = null;
                            AutomationProperties.SetHelpText(view, string.Empty);
                        }
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
            var newDocumentName = BuildNewDocumentName();
            documentViewModel.Open(documentFactory.NewDocument(text, newDocumentName), isNew: true);
            MDI.Open(documentViewModel);
            documentViewModel.Update();
        }

        private string BuildNewDocumentName()
        {
            var newDocumentName = NewDocumentDefaultName;
            var newItemNo = ++numberOfNewDocuments;
            newDocumentName += newItemNo != 1 ? " (" + newItemNo + ")" : "";
            return newDocumentName;
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
            if (IsWorking) return;
            var path = dialogService.GetFileOpenPath("Open a markdown document.", Constants.ExtensionFilter + "|Any File (*.*)|*.*");
            if (path == null)
                return;

            OpenDocument(path);
        }

        public void OpenDocument(IEnumerable<string> filenames)
        {
            if (IsWorking) return;
            if (filenames == null) return;

            foreach (var fn in filenames)
            {
                eventAggregator.Publish(new FileOpenEvent(fn));
            }
        }

        private async Task SaveDocument()
        {
            if (IsWorking) return;
            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                using (DoingWork(string.Format("Saving {0}", doc.MarkpadDocument.Title)))
                {
                    await doc.Save();
                }
            }
        }

        private async Task SaveDocumentAs()
        {
            if (IsWorking) return;
            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                using (DoingWork(string.Format("Saving {0}", doc.MarkpadDocument.Title)))
                {
                    await doc.SaveAs();
                }
            }
        }

        public async Task SaveAllDocuments()
        {
            if (IsWorking) return;
            using (DoingWork(string.Format("Saving all documents")))
            {
                foreach (DocumentViewModel doc in MDI.Items)
                {
                    await doc.Save();
                }
            }
        }

        public void CloseDocument()
        {
            if (IsWorking) return;
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
            OpenHelpDocument("Markdown Help", "MarkdownHelp");
            OpenHelpDocument("MarkPad Help", "MarkPadHelp");
        }

        private void OpenHelpDocument(string title, string content)
        {
            if (!IsDocumentAlreadyOpen(title))
            {
                var documentViewModel = documentViewModelFactory();
                documentViewModel.Open(documentFactory.CreateHelpDocument(title, GetHelpText(content)));
                MDI.Open(documentViewModel);
            }
        }

        private bool IsDocumentAlreadyOpen(string screenName)
        {
            return MDI.Items.Any(item => item.DisplayName == screenName);
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

        private async Task PublishDocument()
        {
            var doc = MDI.ActiveItem as DocumentViewModel;

            if (doc == null) return;
            using (DoingWork(string.Format("Publishing {0}", doc.MarkpadDocument.Title)))
            {
                await doc.Publish();
            }
        }

        private async Task OpenFromWeb()
        {
            var result = await openDocumentFromWeb.Open();
            if (result.Success != true)
                return;

            var postId = result.SelectedPost.postid != null ? result.SelectedPost.postid.ToString() : null;
            var title = result.SelectedPost.title;

            var metaWebLogItem = new WebDocumentItem(null, eventAggregator, postId, title, result.SelectedBlog);

            await OpenDocument(metaWebLogItem, title, () => documentFactory.OpenBlogPost(result.SelectedBlog, postId, title));
        }

        public void Handle(SettingsCloseEvent message)
        {
            CurrentState = "HideSettings";
        }

        public async void Handle(FileOpenEvent message)
        {
            if (!fileSystem.File.Exists(message.Path)) return;
            var fileSystemSiteItem = new FileSystemSiteItem(eventAggregator, fileSystem, message.Path);

            await OpenDocument(fileSystemSiteItem, message.Path, () => documentFactory.OpenDocument(message.Path));
        }

        public async void Handle(OpenFromWebEvent message)
        {
            var metaWebLogItem = new WebDocumentItem(null, eventAggregator, message.Id, message.Name, message.Blog);

            await OpenDocument(metaWebLogItem, message.Name, () => documentFactory.OpenBlogPost(message.Blog, message.Id, message.Name));
        }

        async Task OpenDocument(ISiteItem siteItem, string documentName, Func<Task<IMarkpadDocument>> openDocument)
        {
            try
            {
                var openedDocs = MDI.Items.Cast<DocumentViewModel>();
                var openedDoc = openedDocs.SingleOrDefault(d => d.MarkpadDocument.IsSameItem(siteItem));

                if (openedDoc != null)
                    MDI.ActivateItem(openedDoc);
                else
                {
                    using (DoingWork(string.Format("Opening {0}", documentName)))
                    {
                        var document = await openDocument();

                        var doc = documentViewModelFactory();
                        doc.Open(document);
                        MDI.Open(doc);
                    }
                }
            }
            catch (Exception ex)
            {
                DoDefaultErrorHandling(ex, string.Format("Open {0}", documentName));
            }
        }

        private void DoDefaultErrorHandling(Exception e, string operation)
        {
            // We don't care about cancelled exceptions
            if (e is TaskCanceledException)
                return;

            dialogService.ShowError((string.IsNullOrEmpty(operation) ? "Error occured" : string.Format("Failed to {0}", operation)), e.Message, null);            
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

            var selectSearch = SearchSettings.SelectSearch || (searchType == SearchType.Next || searchType == SearchType.Prev);

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
