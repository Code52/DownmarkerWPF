using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.Document;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Events;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.MetaWeblog
{
    public class MetaWeblogSiteContext : ISiteContext
    {
        readonly string workingDirectory;
        readonly BlogSetting blog;
        readonly Post post;
        readonly Func<string, IMetaWeblogService> getMetaWeblog;
        readonly IEventAggregator eventAggregator;
        readonly List<string> imagesToSaveOnPublish = new List<string>();
        readonly IPublishService publistService;

        public MetaWeblogSiteContext(
            BlogSetting blog, Post post, 
            Func<string, IMetaWeblogService> getMetaWeblog, 
            IEventAggregator eventAggregator, 
            IPublishService publistService)
        {
            this.blog = blog;
            this.post = post;
            this.getMetaWeblog = getMetaWeblog;
            this.eventAggregator = eventAggregator;
            this.publistService = publistService;
            Items = new ObservableCollection<SiteItemBase>();

            workingDirectory = Path.Combine(Path.GetTempPath(), blog.BlogName);
            IsLoading = true;
            getMetaWeblog(blog.WebAPI)
                .GetRecentPostsAsync(blog, 100)
                .ContinueWith(t=>
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

        public event PropertyChangedEventHandler PropertyChanged;

        public string SaveImage(Bitmap image)
        {
            var imageFileName = SiteContextHelper.GetFileName(post.title, workingDirectory);

            image.Save(Path.Combine(workingDirectory, imageFileName), ImageFormat.Png);

            imagesToSaveOnPublish.Add(imageFileName);

            return imageFileName;
        }

        public string ConvertToAbsolutePaths(string htmlDocument)
        {
            return SiteContextHelper.ConvertToAbsolutePaths(htmlDocument, workingDirectory);
        }

        public ObservableCollection<SiteItemBase> Items { get; private set; }
        public bool IsLoading { get; private set; }
        public bool SupportsSave { get { return true; } }

        public void OpenItem(SiteItemBase selectedItem)
        {
            var metaWebLogItem = selectedItem as MetaWebLogItem;
            if (metaWebLogItem != null) 
                eventAggregator.Publish(new OpenFromWebEvent(metaWebLogItem.Post, blog));
        }

        public bool IsCurrentItem(SiteItemBase siteItemBase)
        {
            var item = siteItemBase as MetaWebLogItem;
            if (item != null)
                return post.title == item.Name;

            return false;
        }

        public bool Save(string displayName, string content)
        {
            var postid = post.postid == null ? null : post.postid.ToString();

            if (imagesToSaveOnPublish.Count > 0)
            {
                var metaWebLog = getMetaWeblog(blog.WebAPI);

                foreach (var imageToUpload in imagesToSaveOnPublish)
                {
                    var response = metaWebLog.NewMediaObject(blog, new MediaObject
                    {
                        name = imageToUpload,
                        type = "image/png",
                        bits = File.ReadAllBytes(Path.Combine(workingDirectory, imageToUpload))
                    });

                    content = content.Replace("/" + imageToUpload, response.url);
                }
                imagesToSaveOnPublish.Clear();
            }

            return publistService. Publish(postid, post.title, displayName, post.categories, blog, content) != null;
        }
    }
}