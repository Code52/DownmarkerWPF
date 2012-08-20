using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Caliburn.Micro;
using CookComputing.XmlRpc;
using MarkPad.Document;
using MarkPad.DocumentSources.MetaWeblog;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Helpers;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Plugins;
using MarkPad.PreviewControl;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources
{
    public class DocumentFactory : IDocumentFactory
    {
        readonly IDialogService dialogService;
        readonly Func<string, IMetaWeblogService> getMetaWeblog;
        readonly IEventAggregator eventAggregator;
        readonly ISiteContextGenerator siteContextGenerator;
        readonly IBlogService blogService;
        readonly Func<OpenFromWebViewModel> openFromWebViewModelFactory;
        readonly IWindowManager windowManager;

        public DocumentFactory(
            IDialogService dialogService, 
            Func<string, IMetaWeblogService> getMetaWeblog, 
            IEventAggregator eventAggregator,
            ISiteContextGenerator siteContextGenerator, 
            IBlogService blogService, 
            Func<OpenFromWebViewModel> openFromWebViewModelFactory,
            IWindowManager windowManager)
        {
            this.dialogService = dialogService;
            this.getMetaWeblog = getMetaWeblog;
            this.eventAggregator = eventAggregator;
            this.siteContextGenerator = siteContextGenerator;
            this.blogService = blogService;
            this.openFromWebViewModelFactory = openFromWebViewModelFactory;
            this.windowManager = windowManager;
        }

        public IMarkpadDocument NewDocument()
        {
            return new NewMarkpadDocument(this, string.Empty);
        }

        public IMarkpadDocument NewDocument(string initalText)
        {
            return new NewMarkpadDocument(this, initalText);
        }

        public IMarkpadDocument CreateHelpDocument(string title, string content)
        {
            return new HelpDocument(title, content, this);
        }

        public Task<IMarkpadDocument> NewMarkdownFile(string path, string markdownContent)
        {
            //TODO async all the things
            var streamWriter = new StreamWriter(path);

            return streamWriter
                .WriteAsync(markdownContent)
                .ContinueWith<IMarkpadDocument>(t =>
                {
                    streamWriter.Dispose();

                    t.PropagateExceptions();

                    var siteContext = siteContextGenerator.GetContext(path);

                    return new FileMarkdownDocument(path, markdownContent, siteContext, this, eventAggregator);
                });
        }

        public Task<IMarkpadDocument> OpenDocument(string path)
        {
            var streamWriter = new StreamReader(path);

            return streamWriter
                .ReadToEndAsync()
                .ContinueWith<IMarkpadDocument>(t =>
                {
                    streamWriter.Dispose();

                    t.PropagateExceptions();

                    var siteContext = siteContextGenerator.GetContext(path);

                    return new FileMarkdownDocument(path, t.Result, siteContext, this, eventAggregator);
                });
        }

        public Task<IMarkpadDocument> PublishDocument(string postId, IMarkpadDocument document)
        {
            var blogs = blogService.GetBlogs();
            if (blogs == null || blogs.Count == 0)
            {
                if (!blogService.ConfigureNewBlog("Publish document"))
                    return TaskEx.FromResult<IMarkpadDocument>(null);
                blogs = blogService.GetBlogs();
                if (blogs == null || blogs.Count == 0)
                    return TaskEx.FromResult<IMarkpadDocument>(null);
            }

            var post = new Post();
            var pd = new Details { Title = document.Title, Categories = post.categories };
            var detailsResult = windowManager.ShowDialog(new PublishDetailsViewModel(pd, blogs));
            if (detailsResult != true)
                return TaskEx.FromResult<IMarkpadDocument>(null);

            return TaskEx.Run(() =>
            {
                var webLogItem = document as WebMarkdownFile;
                var imagesToUpload = webLogItem == null ? new List<string>() : webLogItem.ImagesToSaveOnPublish;
                return CreateOrUpdateMetaWebLogPost(postId, pd.Title, pd.Categories, document.MarkdownContent, imagesToUpload, pd.Blog);
            });
        }

        public Task<IMarkpadDocument> OpenFromWeb()
        {
            var blogs = blogService.GetBlogs();
            if (blogs == null || blogs.Count == 0)
            {
                if (!blogService.ConfigureNewBlog("Open from web"))
                    return TaskEx.FromResult<IMarkpadDocument>(null);
                blogs = blogService.GetBlogs();
                if (blogs == null || blogs.Count == 0)
                    return TaskEx.FromResult<IMarkpadDocument>(null);
            }

            var openFromWeb = openFromWebViewModelFactory();
            openFromWeb.InitializeBlogs(blogs);

            var result = windowManager.ShowDialog(openFromWeb);
            if (result != true)
                return TaskEx.FromResult<IMarkpadDocument>(null);

            var metaWeblogSiteContext = new MetaWeblogSiteContext(openFromWeb.SelectedBlog, getMetaWeblog, eventAggregator);
            var webMarkdownFile = new WebMarkdownFile(openFromWeb.SelectedBlog, openFromWeb.SelectedPost, this, metaWeblogSiteContext);
            return TaskEx.FromResult<IMarkpadDocument>(webMarkdownFile);
        }

        public Task<IMarkpadDocument> OpenBlogPost(BlogSetting blog, Post post)
        {
            var metaWeblogSiteContext = new MetaWeblogSiteContext(blog, getMetaWeblog, eventAggregator);
            var webMarkdownFile = new WebMarkdownFile(blog, post, this, metaWeblogSiteContext);
            return TaskEx.FromResult<IMarkpadDocument>(webMarkdownFile);
        }

        public Task<IMarkpadDocument> SaveDocumentAs(IMarkpadDocument document)
        {
            var path = dialogService.GetFileSavePath("Save As", "*.md", Constants.ExtensionFilter + "|All Files (*.*)|*.*");

            if (string.IsNullOrEmpty(path))
                return TaskEx.FromResult(document);

            return NewMarkdownFile(path, document.MarkdownContent);
        }

        IMarkpadDocument CreateOrUpdateMetaWebLogPost(
            string postid, string postTitle, 
            string[] categories, string content, 
            List<string> imagesToUpload, 
            BlogSetting blog)
        {
            var proxy = getMetaWeblog(blog.WebAPI);

            if (imagesToUpload.Count > 0)
            {
                var metaWebLog = getMetaWeblog(blog.WebAPI);

                foreach (var imageToUpload in imagesToUpload)
                {
                    var response = metaWebLog.NewMediaObject(blog, new MediaObject
                    {
                        name = imageToUpload,
                        type = "image/png",
                        bits = File.ReadAllBytes(imageToUpload)
                    });

                    content = content.Replace("/" + imageToUpload, response.url);
                }
            }


            var newpost = new Post();
            try
            {
                if (string.IsNullOrWhiteSpace(postid))
                {
                    var permalink = postTitle;

                    newpost = new Post
                    {
                        permalink = permalink,
                        title = postTitle,
                        dateCreated = DateTime.Now,
                        description = blog.Language == "HTML" ? DocumentParser.GetBodyContents(content) : content,
                        categories = categories,
                        format = blog.Language
                    };
                    newpost.postid = proxy.NewPost(blog, newpost, true);
                }
                else
                {
                    newpost = proxy.GetPost(postid, blog);
                    newpost.title = postTitle;
                    newpost.description = blog.Language == "HTML" ? DocumentParser.GetBodyContents(content) : content;
                    newpost.categories = categories;
                    newpost.format = blog.Language;

                    proxy.EditPost(postid, blog, newpost, true);
                }
            }
            catch (WebException ex)
            {
                dialogService.ShowError("Error Publishing", ex.Message, "");
            }
            catch (XmlRpcException ex)
            {
                dialogService.ShowError("Error Publishing", ex.Message, "");
            }
            catch (XmlRpcFaultException ex)
            {
                dialogService.ShowError("Error Publishing", ex.Message, "");
            }

            var metaWeblogSiteContext = new MetaWeblogSiteContext(blog, getMetaWeblog, eventAggregator);
            return new WebMarkdownFile(blog, newpost, this, metaWeblogSiteContext);
        }
    }
}