using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Caliburn.Micro;
using MarkPad.DocumentSources.FileSystem;
using MarkPad.Infrastructure.DialogService;

namespace MarkPad.DocumentSources
{
    public class JekyllMarkdownDocument : FileMarkdownDocument
    {
        readonly JekyllSiteContext siteContext;

        public JekyllMarkdownDocument(
            string path, string markdownContent, JekyllSiteContext siteContext, 
            IDocumentFactory documentFactory, IEventAggregator eventAggregator, IDialogService dialogService)
            : base(path, markdownContent, siteContext, documentFactory, eventAggregator, dialogService)
        {
            this.siteContext = siteContext;
        }

        public override string SaveImage(Bitmap image)
        {
            var imageFileName = SiteContextHelper.GetFileName(Title, siteContext.SiteBasePath);

            using (var stream = new FileStream(imageFileName, FileMode.Create))
            {
                image.Save(stream, ImageFormat.Png);
            }

            return "/" + SiteContextHelper.ToRelativePath(siteContext.SiteBasePath, FileName, imageFileName).TrimStart('\\', '/');
        }
    }
}