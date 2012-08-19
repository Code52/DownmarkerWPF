using System;
using System.Threading.Tasks;
using MarkPad.Document;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Plugins;
using MarkPad.Settings.Models;
using Ookii.Dialogs.Wpf;

namespace MarkPad.DocumentSources
{
    public class WebMarkdownFile : MarkpadDocumentBase
    {
        readonly BlogSetting blog;
        readonly Post post;
        readonly Func<string, IMetaWeblogService> getMetaWeblog;
        readonly IDialogService dialogService;

        public WebMarkdownFile(
            BlogSetting blog, Post post, 
            Func<string, IMetaWeblogService> getMetaWeblog, 
            IDialogService dialogService, 
            IDocumentFactory documentFactory) : 
                base(post.title, post.description, blog.BlogName, documentFactory)
        {
            this.blog = blog;
            this.post = post;
            this.getMetaWeblog = getMetaWeblog;
            this.dialogService = dialogService;
        }

        public override Task<IMarkpadDocument> Publish()
        {
            var save = new ButtonExtras(ButtonType.Custom, "Save", "Saves this modified post to your blog");
            var saveAs = new ButtonExtras(ButtonType.Custom, "Save As", "Saves this blog post as a local markdown file");
            var publish = new ButtonExtras(ButtonType.Custom, "Publish As", "Publishes this post to another blog, or as another post");
            dialogService.ShowConfirmationWithCancel("Markpad", "What do you want to do?", "", save, saveAs, publish);

            if (save.WasClicked)
                return base.Publish();
            if (saveAs.WasClicked)
                return SaveAs();
            if (publish.WasClicked)
                return DocumentFactory.PublishDocument(this);

            return TaskEx.FromResult<IMarkpadDocument>(this);
        }

        public override Task<IMarkpadDocument> Save()
        {
            return TaskEx.Run<IMarkpadDocument>(() =>
            {
                var proxy = getMetaWeblog(blog.WebAPI);

                var newpost = proxy.GetPost((string) post.postid, blog);
                newpost.title = post.title;
                newpost.description = blog.Language == "HTML"
                                          ? DocumentParser.GetBodyContents(MarkdownContent)
                                          : MarkdownContent;
                newpost.categories = post.categories;
                newpost.format = blog.Language;

                proxy.EditPost((string) post.postid, blog, newpost, true);

                return new WebMarkdownFile(blog, newpost, getMetaWeblog, dialogService, DocumentFactory);
            });
        }
    }
}