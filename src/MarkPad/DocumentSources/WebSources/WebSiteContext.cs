using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.Document.Events;
using MarkPad.DocumentSources.MetaWeblog;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Plugins;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.WebSources
{
    public abstract class WebSiteContext : ISiteContext
    {
        readonly string workingDirectory;
        ObservableCollection<ISiteItem> items;
        readonly BlogSetting blog;
        readonly IWebDocumentService webDocumentService;
        readonly IEventAggregator eventAggregator;

        protected WebSiteContext(BlogSetting blog, IWebDocumentService webDocumentService, IEventAggregator eventAggregator)
        {
            this.blog = blog;
            this.webDocumentService = webDocumentService;
            this.eventAggregator = eventAggregator;
            workingDirectory = Path.Combine(Path.GetTempPath(), blog.BlogName);
        }

        public string WorkingDirectory { get { return workingDirectory; } }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<ISiteItem> Items
        {
            get
            {
                if (items == null)
                {
                    IsLoading = true;
                    items = new ObservableCollection<ISiteItem>();
                    GetItems()
                        .ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
                                return;
                            }
                            foreach (var otherPosts in t.Result)
                            {
                                var postid = (string) otherPosts.postid;
                                var webDocumentItem = new WebDocumentItem(webDocumentService, eventAggregator, postid, otherPosts.title, Blog);
                                Items.Add(webDocumentItem);
                            }
                            IsLoading = false;
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                return items;
            }
        }

        protected abstract Task<Post[]> GetItems();

        public bool IsLoading { get; private set; }

        public BlogSetting Blog
        {
            get { return blog; }
        }

        public void OpenItem(ISiteItem selectedItem)
        {
            var webDocumentItem = selectedItem as WebDocumentItem;
            if (webDocumentItem != null)
                eventAggregator.Publish(new OpenFromWebEvent(webDocumentItem.Id, webDocumentItem.Name, Blog));
        }
    }
}