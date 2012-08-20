using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using MarkPad.Document;
using MarkPad.DocumentSources;
using MarkPad.Events;
using MarkPad.Infrastructure.DialogService;
using MarkPad.PreviewControl;
using MarkPad.Settings.UI;
using MarkPad.Updater;

namespace MarkPad
{
    internal class ShellViewModel : Conductor<IScreen>, IHandle<FileOpenEvent>, IHandle<SettingsCloseEvent>
    {
        private const string ShowSettingsState = "ShowSettings";
        private readonly IEventAggregator eventAggregator;
        private readonly IDialogService dialogService;
        private readonly Func<DocumentViewModel> documentViewModelFactory;

        public ShellViewModel(
            IDialogService dialogService,
            IEventAggregator eventAggregator,
            MdiViewModel mdi,
            SettingsViewModel settingsViewModel,
            UpdaterViewModel updaterViewModel,
            Func<DocumentViewModel> documentViewModelFactory, 
            IDocumentFactory documentFactory)
        {
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            MDI = mdi;
            Updater = updaterViewModel;
            this.documentViewModelFactory = documentViewModelFactory;
            this.documentFactory = documentFactory;

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
                    documentFactory
                        .OpenDocument(message.Path)
                        .ContinueWith(t =>
                        {
                            var viewModel = documentViewModelFactory();

                            viewModel.Open(t.Result);
                            MDI.Open(viewModel);

                        }, TaskScheduler.FromCurrentSynchronizationContext());
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

            return openedDocs.FirstOrDefault(doc => doc != null && doc.MarkpadDocument is FileMarkdownDocument && filename.Equals(((FileMarkdownDocument)doc.MarkpadDocument).FileName));
        }

        public void ShowHelp()
        {
            var documentViewModel = documentViewModelFactory();
            documentViewModel.Open(documentFactory.CreateHelpDocument("Markdown Help", GetHelpText("MarkdownHelp")));
            MDI.Open(documentViewModel);

            documentViewModel = documentViewModelFactory();
            documentViewModel.Open(documentFactory.CreateHelpDocument("MarkPad Help", GetHelpText("MarkdownHelp")));
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
                doc.Publish();
            }
        }

        public void OpenFromWeb()
        {
            documentFactory.OpenFromWeb()
                .ContinueWith(t=>
                {
                    if (t.IsCanceled || t.Result == null) return;

                    var doc = documentViewModelFactory();
                    doc.Open(t.Result);
                    MDI.Open(doc);
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void Handle(SettingsCloseEvent message)
        {
            CurrentState = "HideSettings";
        }
    }
}
