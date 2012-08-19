using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using MarkPad.DocumentSources.MetaWeblog;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Plugins;
using MarkPad.Settings.Models;
using Ookii.Dialogs.Wpf;

namespace MarkPad.DocumentSources
{
    public class WebMarkdownFile : MarkpadDocumentBase
    {
        Post post;
        readonly IDialogService dialogService;
        readonly MetaWeblogSiteContext siteContext;
        readonly List<string> imagesToSaveOnPublish = new List<string>();

        public WebMarkdownFile(
            BlogSetting blog, Post post, 
            IDialogService dialogService, 
            IDocumentFactory documentFactory,
            MetaWeblogSiteContext siteContext)
            : base(post.title, post.description, blog.BlogName, documentFactory)
        {
            this.post = post;
            this.dialogService = dialogService;
            this.siteContext = siteContext;
        }

        public List<string> ImagesToSaveOnPublish
        {
            get { return imagesToSaveOnPublish; }
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

        public override string SaveImage(Bitmap image)
        {
            var imageFileName = SiteContextHelper.GetFileName(post.title, siteContext.WorkingDirectory);

            image.Save(Path.Combine(siteContext.WorkingDirectory, imageFileName), ImageFormat.Png);

            ImagesToSaveOnPublish.Add(imageFileName);

            return imageFileName;
        }

        public override string ConvertToAbsolutePaths(string htmlDocument)
        {
            return SiteContextHelper.ConvertToAbsolutePaths(htmlDocument, siteContext.WorkingDirectory);
        }

        public override Task<IMarkpadDocument> Save()
        {
            return DocumentFactory.PublishDocument(this);
        }
    }
}