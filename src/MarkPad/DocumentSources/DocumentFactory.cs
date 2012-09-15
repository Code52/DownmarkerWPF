using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.DocumentSources.FileSystem;
using MarkPad.DocumentSources.GitHub;
using MarkPad.DocumentSources.MetaWeblog;
using MarkPad.DocumentSources.WebSources;
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
        readonly IEventAggregator eventAggregator;
        readonly ISiteContextGenerator siteContextGenerator;
        readonly IBlogService blogService;
        readonly Func<OpenFromWebViewModel> openFromWebViewModelFactory;
        readonly IWindowManager windowManager;
        readonly Lazy<IWebDocumentService> webDocumentService;

        public DocumentFactory(
            IDialogService dialogService, 
            IEventAggregator eventAggregator,
            ISiteContextGenerator siteContextGenerator, 
            IBlogService blogService, 
            Func<OpenFromWebViewModel> openFromWebViewModelFactory,
            IWindowManager windowManager, 
            Lazy<IWebDocumentService> webDocumentService)
        {
            this.dialogService = dialogService;
            this.eventAggregator = eventAggregator;
            this.siteContextGenerator = siteContextGenerator;
            this.blogService = blogService;
            this.openFromWebViewModelFactory = openFromWebViewModelFactory;
            this.windowManager = windowManager;
            this.webDocumentService = webDocumentService;
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

                    return new FileMarkdownDocument(path, markdownContent, siteContext, this, eventAggregator, dialogService);
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

                    return new FileMarkdownDocument(path, t.Result, siteContext, this, eventAggregator, dialogService);
                });
        }

        /// <summary>
        /// Publishes any document
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="document"></param>
        /// <returns></returns>
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

            var categories = new List<string>();
            var webDocument = document as WebDocument;
            if (webDocument != null)
                categories = webDocument.Categories;
            var pd = new Details { Title = document.Title, Categories = categories.ToArray()};
            var detailsResult = windowManager.ShowDialog(new PublishDetailsViewModel(pd, blogs));
            if (detailsResult != true)
                return TaskEx.FromResult<IMarkpadDocument>(null);

            var newDocument = new WebDocument(pd.Blog, null, pd.Title, document.MarkdownContent, this,
                                              webDocumentService.Value,
                                              siteContextGenerator.GetWebContext(pd.Blog));

            return newDocument.Save();
        }

        public async Task<IMarkpadDocument> OpenFromWeb()
        {
            var blogs = blogService.GetBlogs();
            if (blogs == null || blogs.Count == 0)
            {
                if (!blogService.ConfigureNewBlog("Open from web"))
                    return null;
                blogs = blogService.GetBlogs();
                if (blogs == null || blogs.Count == 0)
                    return null;
            }

            var openFromWeb = openFromWebViewModelFactory();
            openFromWeb.InitializeBlogs(blogs);

            var result = windowManager.ShowDialog(openFromWeb);
            if (result != true)
                return null;

            var selectedPost = openFromWeb.SelectedPost;
            var postid = (string) selectedPost.postid;
            var title = selectedPost.title;
            var blog = openFromWeb.SelectedBlog;
            var documentService = webDocumentService.Value;
            var content = await documentService.GetDocumentContent(blog, postid);
            var webSiteContext = siteContextGenerator.GetWebContext(blog);
            return new WebDocument(blog, postid, title, content, this, documentService, webSiteContext);
        }

        public async Task<IMarkpadDocument> OpenBlogPost(BlogSetting blog, string id, string name)
        {
            var metaWeblogSiteContext = siteContextGenerator.GetWebContext(blog);

            var content = await webDocumentService.Value.GetDocumentContent(blog, id);

            var webMarkdownFile = new WebDocument(blog, id, name, content, this, 
                webDocumentService.Value, metaWeblogSiteContext);
            return webMarkdownFile;
        }

        public Task<IMarkpadDocument> SaveDocumentAs(IMarkpadDocument document)
        {
            var path = dialogService.GetFileSavePath("Save As", "*.md", Constants.ExtensionFilter + "|All Files (*.*)|*.*");

            if (string.IsNullOrEmpty(path))
                return TaskEx.FromResult(document);

            return NewMarkdownFile(path, document.MarkdownContent);
        }
    }
}