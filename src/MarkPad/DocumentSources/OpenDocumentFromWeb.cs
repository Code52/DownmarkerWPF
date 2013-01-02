using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.DocumentSources.MetaWeblog;

namespace MarkPad.DocumentSources
{
    public class OpenDocumentFromWeb : IOpenDocumentFromWeb
    {
        readonly IBlogService blogService;
        readonly Func<OpenFromWebViewModel> openFromWebViewModelFactory;
        readonly IWindowManager windowManager;

        public OpenDocumentFromWeb(IBlogService blogService, Func<OpenFromWebViewModel> openFromWebViewModelFactory, IWindowManager windowManager)
        {
            this.blogService = blogService;
            this.openFromWebViewModelFactory = openFromWebViewModelFactory;
            this.windowManager = windowManager;
        }

        public Task<OpenDocumentFromWebResult> Open()
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

            var openDocumentFromWebResult = new OpenDocumentFromWebResult
                                            {
                                                Success = result,
                                                SelectedPost = openFromWeb.SelectedPost,
                                                SelectedBlog = openFromWeb.SelectedBlog
                                            };
            return TaskEx.FromResult(openDocumentFromWebResult);
        }
    }
}