using System;
using System.Collections.Generic;
using Caliburn.Micro;
using MarkPad.About;
using MarkPad.Document;
using MarkPad.Framework;
using MarkPad.Framework.Events;
using MarkPad.MDI;
using MarkPad.Metaweblog;
using MarkPad.OpenFromWeb;
using MarkPad.PublishDetails;
using MarkPad.Services.Interfaces;
using MarkPad.Settings;
using MarkPad.Updater;
using Ookii.Dialogs.Wpf;
using System.Linq;
using wyDay.Controls;

namespace MarkPad.Shell
{
    internal class ShellViewModel : Conductor<IScreen>, IHandle<FileOpenEvent>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly Func<UpdaterViewModel> updaterCreator;
        private readonly IDialogService dialogService;
        private readonly IWindowManager windowManager;
        private readonly ISettingsService settingsService;
        private readonly Func<DocumentViewModel> documentCreator;
        private readonly Func<SettingsViewModel> settingsCreator;
        private readonly Func<AboutViewModel> aboutCreator;
        private readonly Func<OpenFromWebViewModel> openFromWebCreator;
        
        public ShellViewModel(
            IDialogService dialogService,
            IWindowManager windowManager,
            ISettingsService settingsService,
            IEventAggregator eventAggregator,
            MDIViewModel mdi,
            UpdaterViewModel updaterViewModel,
            Func<DocumentViewModel> documentCreator,
            Func<SettingsViewModel> settingsCreator,
            Func<AboutViewModel> aboutCreator,
            Func<OpenFromWebViewModel> openFromWebCreator)
        {
            this.eventAggregator = eventAggregator;
            this.updaterCreator = updaterCreator;
            this.dialogService = dialogService;
            this.windowManager = windowManager;
            this.settingsService = settingsService;
            this.MDI = mdi;
            Updater = updaterViewModel;
            this.documentCreator = documentCreator;
            this.settingsCreator = settingsCreator;
            this.aboutCreator = aboutCreator;
            this.openFromWebCreator = openFromWebCreator;
            
			InitialiseDefaultSettings();
            ActivateItem(mdi);
        }

		private void InitialiseDefaultSettings()
		{
			settingsService.SetAsDefault(SettingsViewModel.FontFamilySettingsKey, "Segoe UI");
		}
        public override string DisplayName
        {
            get { return "MarkPad"; }
            set { }
        }

        public MDIViewModel MDI { get; private set; }
        public UpdaterViewModel Updater { get; set; }

        public override void CanClose(Action<bool> callback)
        {
            base.CanClose(callback);
            settingsService.Save();
        }

        public void Exit()
        {
            this.TryClose();
        }

        public void NewDocument()
        {
            MDI.Open(documentCreator());
        }

        public void NewJekyllDocument()
        {
            var creator = documentCreator();
            creator.Document.BeginUpdate();
            creator.Document.Text = CreateJekyllHeader();
            creator.Document.EndUpdate();
            MDI.Open(creator);
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
            foreach (var fn in filenames)
            {
                DocumentViewModel openedDoc = GetOpenedDocument(fn);

                if (openedDoc != null)
                    MDI.ActivateItem(openedDoc);
                else
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

        public void SaveAllDocuments()
        {
            foreach (DocumentViewModel doc in MDI.Items)
            {
                doc.Save();
            }
        }

        public void Handle(FileOpenEvent message)
        {
            var doc = documentCreator();
            doc.Open(message.Path);
            MDI.Open(doc);
        }

        public void ShowSettings()
        {
            windowManager.ShowDialog(settingsCreator());
        }

        public void ShowAbout()
        {
            windowManager.ShowDialog(aboutCreator());
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
            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                doc.Print();
            }
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

            IEnumerable<DocumentViewModel> openedDocs = MDI.Items.Cast<DocumentViewModel>();

            return openedDocs.Where(doc => doc != null && filename.Equals(doc.FileName)).FirstOrDefault();
        }
        private DocumentView GetDocument()
        {
            return (MDI.ActiveItem as DocumentViewModel)
                .Evaluate(d => d.GetView() as DocumentView);
        }

        public void ToggleBold()
        {
            GetDocument()
                .ExecuteSafely(v => v.ToggleBold());
        }

        public void ToggleItalic()
        {
            GetDocument()
                .ExecuteSafely(v => v.ToggleItalic());
        }

        public void ToggleCode()
        {
            GetDocument()
                .ExecuteSafely(v => v.ToggleCode());
        }

        public void SetHyperlink()
        {
            GetDocument()
                .ExecuteSafely(v => v.SetHyperlink());
        }



        public void PublishDocument()
        {
            var blogs = settingsService.Get<List<BlogSetting>>("Blogs");
            if (blogs == null || blogs.Count == 0)
            {
                dialogService.ShowError("Error Publishing Post", "No blogs available to publish to.", "");
                return;
            }

            var doc = MDI.ActiveItem as DocumentViewModel;
            if (doc != null)
            {
                var pd = new Details {Title = doc.Post.title, Categories = doc.Post.categories};
                var detailsResult = windowManager.ShowDialog(new PublishDetailsViewModel(pd, blogs));
                if (detailsResult != true)
                    return;

                doc.Publish(doc.Post.postid == null ? null : doc.Post.postid.ToString(), pd.Title, pd.Categories,
                            pd.Blog);
            }
        }

        public void OpenFromWeb()
        {
            var blogs = settingsService.Get<List<BlogSetting>>("Blogs");
            if (blogs == null || blogs.Count == 0)
            {
                var setupBlog = dialogService.ShowConfirmation(
					"Open from web",
					"Do you want to configure a blog site?",
					"No blog sites have been configured. To open a document from the web, MarkPad first needs to be integrated with a blog site.");

				if (!setupBlog)
					return;

				var settings = settingsCreator();
				if (!settings.AddBlog())
					return;                    
            }


            var openFromWeb = openFromWebCreator();
            openFromWeb.InitializeBlogs(blogs);

            var result = windowManager.ShowDialog(openFromWeb);
            if (result != true)
                return;

            var post = settingsService.Get<Post>("CurrentPost");

            var doc = documentCreator();
            doc.OpenFromWeb(post);
            MDI.Open(doc);
        }
    }


}
