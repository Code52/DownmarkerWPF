using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Action = System.Action;

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
            SaveDocumentCommand = new DelegateCommand<object>(o=>SaveDocument(), o=> CanSaveDocument);

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
        bool isWorking;

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
        public bool IsWorking
        {
            get { return isWorking; }
            private set
            {
                isWorking = value;
                ((DelegateCommand<object>)SaveDocumentCommand).RaiseCanExecuteChanged();
            }
        }

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
            documentViewModel.Open(documentFactory.NewDocument(text), isNew: true);
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

        public async void SaveDocument()
        {
            if (IsWorking) return;
            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                using (DoingWork(string.Format("Saving {0}", doc.MarkpadDocument.Title)))
                {
                    await doc.Save();
                    await TaskEx.Delay(10000);
                }
            }
        }

        private bool CanSaveDocument
        {
            get { return !IsWorking; }
        }

        public async void SaveAsDocument()
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

        public void SaveAllDocuments()
        {
            if (IsWorking) return;
            using (DoingWork(string.Format("Saving all documents")))
            {
                foreach (DocumentViewModel doc in MDI.Items)
                {
                    doc.Save();
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

        public async void PublishDocument()
        {
            var doc = MDI.ActiveItem as DocumentViewModel;

            if (doc == null) return;
            using (DoingWork(string.Format("Publishing {0}", doc.MarkpadDocument.Title)))
            {
                await doc.Publish();
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

        public async void Handle(FileOpenEvent message)
        {
            if (!fileSystem.File.Exists(message.Path)) return;

            var openedDocs = MDI.Items.Cast<DocumentViewModel>();
            var fileSystemSiteItem = new FileSystemSiteItem(eventAggregator, fileSystem, message.Path);
            var openedDoc = openedDocs.SingleOrDefault(d => d.MarkpadDocument.IsSameItem(fileSystemSiteItem));

            if (openedDoc != null)
                MDI.ActivateItem(openedDoc);
            else
            {
                using (DoingWork(string.Format("Opening {0}", message.Path)))
                {
                    await documentFactory.OpenDocument(message.Path).ContinueWith(OpenDocumentResult, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

        public async void Handle(OpenFromWebEvent message)
        {
            using (DoingWork(string.Format("Opening {0}", message.Name)))
            {
                var openedDocs = MDI.Items.Cast<DocumentViewModel>();
                var metaWebLogItem = new WebDocumentItem(null, eventAggregator, message.Id, message.Name, message.Blog);
                var openedDoc = openedDocs.SingleOrDefault(d => d.MarkpadDocument.IsSameItem(metaWebLogItem));

                if (openedDoc != null)
                    MDI.ActivateItem(openedDoc);
                else
                {
                    await documentFactory
                        .OpenBlogPost(message.Blog, message.Id, message.Name)
                        .ContinueWith(OpenDocumentResult, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

        void OpenDocumentResult(Task<IMarkpadDocument> t)
        {
            if (t.IsFaulted && t.Exception != null)
            {
                var aggregateException = t.Exception;
                dialogService.ShowError("Failed to open document", aggregateException.InnerException.Message, null);
                return;
            }
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

        public ICommand SaveDocumentCommand { get; private set; }

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

    /// <summary>
    /// A command that calls the specified delegate when the command is executed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelegateCommand<T> : ICommand
    {
        private readonly Func<T, bool> _canExecuteMethod;
        private readonly Action<T> _executeMethod;
        private bool _isExecuting;

        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, null)
        {
        }

        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            if ((executeMethod == null) && (canExecuteMethod == null))
            {
                throw new ArgumentNullException("executeMethod", @"Execute Method cannot be null");
            }
            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        bool ICommand.CanExecute(object parameter)
        {
            return !_isExecuting && CanExecute((T)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            _isExecuting = true;
            try
            {
                RaiseCanExecuteChanged();
                Execute((T)parameter);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public bool CanExecute(T parameter)
        {
            if (_canExecuteMethod == null)
                return true;

            return _canExecuteMethod(parameter);
        }

        public void Execute(T parameter)
        {
            _executeMethod(parameter);
        }
    }
}
