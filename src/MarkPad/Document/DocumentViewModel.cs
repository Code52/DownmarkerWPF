using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit.Document;
using MarkPad.Document.Controls;
using MarkPad.Document.Search;
using MarkPad.Document.SpellCheck;
using MarkPad.Events;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Plugins;
using MarkPad.PreviewControl;
using MarkPad.Settings;
using MarkPad.Settings.Models;
using Ookii.Dialogs.Wpf;
using Action = System.Action;

namespace MarkPad.Document
{
    public class DocumentViewModel : Screen, IHandle<SettingsChangedEvent>
    {
        static readonly ILog Log = LogManager.GetLog(typeof(DocumentViewModel));
        const double ZoomDelta = 0.1;
        double zoomLevel = 1;

        readonly IDialogService dialogService;
        readonly IWindowManager windowManager;
        readonly ISettingsProvider settingsProvider;
        readonly IDocumentParser documentParser;
        readonly IShell shell;

        readonly TimeSpan delay = TimeSpan.FromSeconds(0.5);
        readonly DispatcherTimer timer;

        readonly Regex wordCountRegex = new Regex(@"[\S]+", RegexOptions.Compiled);

        public DocumentViewModel(
            IDialogService dialogService, 
            IWindowManager windowManager,
            ISettingsProvider settingsProvider,
			IDocumentParser documentParser,
            ISpellCheckProvider spellCheckProvider,
            ISearchProvider searchProvider,
            IShell shell)
        {
            SpellCheckProvider = spellCheckProvider;
            this.dialogService = dialogService;
            this.windowManager = windowManager;
            this.settingsProvider = settingsProvider;
            this.documentParser = documentParser;
            this.shell = shell;
            SearchProvider = searchProvider;

            FontSize = GetFontSize();
            IsColorsInverted = GetIsColorsInverted();
            IndentType = settingsProvider.GetSettings<MarkPadSettings>().IndentType;
            
            Original = "";
            Document = new TextDocument();
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
            if (MarkpadDocument == null)
                return;
            timer.Stop();

			Task.Factory.StartNew(text => documentParser.Parse(text.ToString()), Document.Text)
            .ContinueWith(s =>
            {
                if (s.IsFaulted)
                {
                    Log.Error(s.Exception);
                    return;
                }

                var result = MarkpadDocument.ConvertToAbsolutePaths(s.Result);

                Render = result;
                UpdateWordCount();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void Open(IMarkpadDocument document, bool isNew = false)
        {
            MarkpadDocument = document;
            Document.Text = document.MarkdownContent ?? string.Empty;
            Original = isNew ? null : document.MarkdownContent;

            Update();
        }

        public void Update()
        {
            timer.Stop();
            timer.Start();
            NotifyOfPropertyChange(() => HasChanges);
            NotifyOfPropertyChange(() => DisplayName);
        }

        public Task<bool> SaveAs()
        {
            MarkpadDocument.MarkdownContent = Document.Text;
            return MarkpadDocument
                .SaveAs()
                .ContinueWith(t=>
                {
                    if (t.IsFaulted || t.IsCanceled)
                        return false;

                    MarkpadDocument = t.Result;
                    Original = MarkpadDocument.MarkdownContent;
                    Document.Text = MarkpadDocument.MarkdownContent;
                    return true;
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public Task Publish()
        {
            MarkpadDocument.MarkdownContent = Document.Text;

            return MarkpadDocument.Publish()
                .ContinueWith(t =>
                {
                    if (t.Result != null)
                    {
                        MarkpadDocument = t.Result;
                        Original = MarkpadDocument.MarkdownContent;
                        Document.Text = MarkpadDocument.MarkdownContent;
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public Task<bool> Save()
        {
            if (Document.Text == Original)
                return TaskEx.FromResult(false);

            MarkpadDocument.MarkdownContent = Document.Text;

            //TODO async all the things
            var dispatcher = Dispatcher.CurrentDispatcher;
            return MarkpadDocument.Save()
                .ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        return false;

                    if (t.IsFaulted)
                    {
                        var saveResult = (bool?) dispatcher.Invoke(new Action(() =>
                        {
                            dialogService.ShowConfirmation(
                                "MarkPad", "Cannot save file",
                                "You may not have permission to save this file to the selected location, or the location may not be currently available. Error: " + t.Exception.InnerException.Message,
                                new ButtonExtras(ButtonType.Yes, "Select a different location", "Save this file to a different location."),
                                new ButtonExtras(ButtonType.No, "Cancel", ""));
                        }));

                        return saveResult == true && SaveAs().Result;
                    }

                    dispatcher.Invoke(new Action(() =>
                    {
                        MarkpadDocument = t.Result;
                        Original = MarkpadDocument.MarkdownContent;
                        Document.Text = MarkpadDocument.MarkdownContent;
                    }));
                    return true;
                });
        }

        public TextDocument Document { get; private set; }
		public string MarkdownContent { get { return Document.Text; } }

        public string Original { get; set; }

        public string Render { get; private set; }

        public bool HasChanges
        {
            get { return Original != Document.Text; }
        }

        public override string DisplayName
        {
            get { return MarkpadDocument == null ? string.Empty : MarkpadDocument.Title; }
        }

        public IMarkpadDocument MarkpadDocument { get; private set; }

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
            if (!HasChanges)
            {
                CheckAndCloseView();
                callback(true);
                return;
            }

            var saveResult = dialogService.ShowConfirmationWithCancel("MarkPad", "Save file", "Do you want to save the changes to '" + MarkpadDocument.Title + "'?",
                new ButtonExtras(ButtonType.Yes, "Save",
                    string.IsNullOrEmpty(MarkpadDocument.SaveLocation) ? "The file has not been saved yet" : "The file will be saved to " + Path.GetFullPath(MarkpadDocument.SaveLocation)),
                new ButtonExtras(ButtonType.No, "Discard", "Discard the changes to this file"),
                new ButtonExtras(ButtonType.Cancel, "Cancel", "Cancel closing the application")
            );

            if (saveResult == true)
            {
                var finishedWork = shell.DoingWork(string.Format("Saving {0}", MarkpadDocument.Title));
                Save()
                    .ContinueWith(t =>
                    {
                        finishedWork.Dispose();
                        if (t.IsCompleted)
                            CheckAndCloseView();
                        else
                            callback(false);
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                return;
            }

            if (saveResult == false)
            {
                CheckAndCloseView();
                callback(true);
                return;
            }

            callback(false);
        }

        private void CheckAndCloseView()
        {
            if (SpellCheckProvider != null)
                SpellCheckProvider.Disconnect();
            var disposableSiteContext = MarkpadDocument.SiteContext as IDisposable;
            if (disposableSiteContext != null)
                disposableSiteContext.Dispose();
        }

        public bool DistractionFree { get; set; }

        public int WordCount { get; private set; }

        public double FontSize { get; private set; }

        public bool IsColorsInverted { get; set; }

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

        public IndentType IndentType { get; private set; }

        /// <summary>
        /// Get the font size that was set in the settings.
        /// </summary>
        /// <returns>Font size.</returns>
        private int GetFontSize()
        {
            return Constants.FONT_SIZE_ENUM_ADJUSTMENT + (int)settingsProvider.GetSettings<MarkPadSettings>().FontSize;
        }

        private bool GetIsColorsInverted()
        {
            return settingsProvider.GetSettings<MarkPadSettings>().IsEditorColorsInverted;
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

        public void RefreshColors()
        {
            IsColorsInverted = GetIsColorsInverted();
        }

        public void Handle(SettingsChangedEvent message)
        {
            IndentType = settingsProvider.GetSettings<MarkPadSettings>().IndentType;            
        }

        public DocumentView View
        {
            get { return (DocumentView)GetView(); }
        }

        public ISpellCheckProvider SpellCheckProvider { get; private set; }

        protected override void OnViewLoaded(object view)
        {
            SpellCheckProvider.Initialise((DocumentView)view);
            SearchProvider.Initialise((DocumentView)view);
            base.OnViewLoaded(view);
            NotifyOfPropertyChange(()=>View);
        }

        protected override void OnDeactivate(bool close)
        {
            if (View != null)
                View.siteView.UndoRename();
        }

        public ISearchProvider SearchProvider { get; private set; }

        public bool Overtype { get; set; }
    }
}
