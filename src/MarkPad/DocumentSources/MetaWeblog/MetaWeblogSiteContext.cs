using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.Document.Events;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Plugins;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.MetaWeblog
{
    public class MetaWeblogSiteContext : ISiteContext
    {
        readonly string workingDirectory;
        readonly BlogSetting blog;
        readonly Func<string, IMetaWeblogService> getMetaWeblog;
        readonly IEventAggregator eventAggregator;
        ObservableCollection<ISiteItem> items;

        public MetaWeblogSiteContext(
            BlogSetting blog,
            Func<string, IMetaWeblogService> getMetaWeblog,
            IEventAggregator eventAggregator)
        {
            this.blog = blog;
            this.getMetaWeblog = getMetaWeblog;
            this.eventAggregator = eventAggregator;

            workingDirectory = Path.Combine(Path.GetTempPath(), blog.BlogName);
            IsLoading = true;
        }

        public string ConvertToAbsolutePaths(string htmlDocument)
        {
            return SiteContextHelper.ConvertToAbsolutePaths(htmlDocument, workingDirectory);
        }

        public ObservableCollection<ISiteItem> Items
        {
            get
            {
                if (items == null)
                {
                    items = new ObservableCollection<ISiteItem>();
                    getMetaWeblog(blog.WebAPI)
                        .GetRecentPostsAsync(blog, 100)
                        .ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
                                return;
                            }
                            foreach (var otherPosts in t.Result)
                            {
                                Items.Add(new MetaWebLogItem(getMetaWeblog, eventAggregator, otherPosts, blog));
                            }
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                return items;
            }
        }

        public bool IsLoading { get; private set; }

        public string WorkingDirectory { get { return workingDirectory; } }

        public void OpenItem(ISiteItem selectedItem)
        {
            var metaWebLogItem = selectedItem as MetaWebLogItem;
            if (metaWebLogItem != null)
                eventAggregator.Publish(new OpenFromWebEvent(metaWebLogItem.Post, blog));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }
    }
}