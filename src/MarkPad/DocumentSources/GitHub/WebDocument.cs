using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using MarkPad.DocumentSources.MetaWeblog;
using MarkPad.DocumentSources.WebSources;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Plugins;
using MarkPad.Settings.Models;
using Ookii.Dialogs.Wpf;

namespace MarkPad.DocumentSources.GitHub
{
    public class WebDocument : MarkpadDocumentBase
    {
        readonly BlogSetting blog;
        string id;
        readonly IWebDocumentService webDocumentService;
        readonly WebSiteContext siteContext;
        readonly List<string> imagesToSaveOnPublish = new List<string>();
        readonly List<string> categories = new List<string>();

        public WebDocument(
            BlogSetting blog,
            string id,
            string title,
            string content,
            IDocumentFactory documentFactory,
            IWebDocumentService webDocumentService,
            WebSiteContext siteContext) :
            base(title, content, blog.BlogName, documentFactory)
        {
            this.blog = blog;
            this.id = id;
            this.webDocumentService = webDocumentService;
            this.siteContext = siteContext;
        }

        public string Id
        {
            get { return id; }
        }

        public override async Task<IMarkpadDocument> Save()
        {
            id = await webDocumentService.SaveDocument(blog, this);
            return this;         
        }

        public override ISiteContext SiteContext
        {
            get
            {
                return siteContext;
            }
        }

        public List<string> ImagesToSaveOnPublish
        {
            get { return imagesToSaveOnPublish; }
        }

        public List<string> Categories
        {
            get { return categories; }
        }

        public override Task<IMarkpadDocument> Publish()
        {
            var save = new ButtonExtras(ButtonType.Yes, "Save", "Saves this modified post to your blog");
            var saveAs = new ButtonExtras(ButtonType.No, "Save As", "Saves this blog post as a local markdown file");
            var publish = new ButtonExtras(ButtonType.Retry, "Publish As", "Publishes this post to another blog, or as another post");

            var service = new DialogMessageService(null)
            {
                Icon = DialogMessageIcon.Question,
                Buttons = DialogMessageButtons.Yes | DialogMessageButtons.No | DialogMessageButtons.Retry | DialogMessageButtons.Cancel,
                Title = "Markpad",
                Text = string.Format("{0} has already been published, what do you want to do?", Title),
                ButtonExtras = new[] { save, saveAs, publish }
            };

            var result = service.Show();
            switch (result)
            {
                case DialogMessageResult.Yes:
                    return Save();
                case DialogMessageResult.No:
                    return SaveAs();
                case DialogMessageResult.Retry:
                    return DocumentFactory.PublishDocument(null, this);
            }

            return TaskEx.FromResult<IMarkpadDocument>(this);
        }

        public override string SaveImage(Bitmap image)
        {
            var imageFileName = SiteContextHelper.GetFileName(Title, siteContext.WorkingDirectory);

            image.Save(Path.Combine(siteContext.WorkingDirectory, imageFileName), ImageFormat.Png);

            ImagesToSaveOnPublish.Add(imageFileName);

            return imageFileName;
        }

        public override string ConvertToAbsolutePaths(string htmlDocument)
        {
            return SiteContextHelper.ConvertToAbsolutePaths(htmlDocument, siteContext.WorkingDirectory);
        }

        public override bool IsSameItem(ISiteItem siteItem)
        {
            var webDocumentItem = siteItem as WebDocumentItem;
            if (webDocumentItem != null)
                return webDocumentItem.Id == Id;

            return false;
        }
    }
}