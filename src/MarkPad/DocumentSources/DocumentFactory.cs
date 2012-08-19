using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Caliburn.Micro;
using CookComputing.XmlRpc;
using MarkPad.Document;
using MarkPad.DocumentSources.MetaWeblog;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Plugins;
using MarkPad.Framework;
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
            return new NewMarkpadDocument(dialogService, this, string.Empty);
        }

        public IMarkpadDocument NewDocument(string initalText)
        {
            return new NewMarkpadDocument(dialogService, this, initalText);
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

                    return new FileMarkdownDocument(path, markdownContent, dialogService, siteContext, this, eventAggregator);
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

                    return new FileMarkdownDocument(path, t.Result, dialogService, siteContext, this, eventAggregator);
                });
        }

        public Task<IMarkpadDocument> PublishDocument(IMarkpadDocument document)
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

            return TaskEx.Run(() => CreateNewWebMarkdownFile(null, pd.Title, pd.Categories, document.MarkdownContent, pd.Blog));
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

            return TaskEx.FromResult<IMarkpadDocument>(new WebMarkdownFile(openFromWeb.SelectedBlog, openFromWeb.SelectedPost, getMetaWeblog, dialogService, this));
        }

        public Task<IMarkpadDocument> SaveDocumentAs(IMarkpadDocument document)
        {
            var path = dialogService.GetFileSavePath("Save As", "*.md", Constants.ExtensionFilter + "|All Files (*.*)|*.*");

            if (string.IsNullOrEmpty(path))
                return TaskEx.FromResult(document);

            return NewMarkdownFile(path, document.MarkdownContent);
        }

        IMarkpadDocument CreateNewWebMarkdownFile(string postid, string postTitle, string[] categories, string content, BlogSetting blog)
        {
            var proxy = getMetaWeblog(blog.WebAPI);

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

            return new WebMarkdownFile(blog, newpost, getMetaWeblog, dialogService, this);
        }
    }
}