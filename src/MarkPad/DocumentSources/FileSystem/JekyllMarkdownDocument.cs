using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Caliburn.Micro;
using MarkPad.Infrastructure;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources.FileSystem
{
    public class JekyllMarkdownDocument : FileMarkdownDocument
    {
        readonly JekyllSiteContext siteContext;

        public JekyllMarkdownDocument(
            string path, string markdownContent, JekyllSiteContext siteContext, 
            IDocumentFactory documentFactory, IEventAggregator eventAggregator, IDialogService dialogService, IFileSystem fileSystem)
            : base(path, markdownContent, siteContext, documentFactory, eventAggregator, dialogService, fileSystem)
        {
            this.siteContext = siteContext;
        }

        public override FileReference SaveImage(Bitmap image)
        {
            var imageFileName = GetFileNameBasedOnTitle(Title, siteContext.SiteBasePath);

            using (var stream = new FileStream(imageFileName, FileMode.Create))
            {
                image.Save(stream, ImageFormat.Png);
            }

            var relativePath = "/" + ToRelativePath(siteContext.SiteBasePath, FileName, imageFileName).TrimStart('\\', '/');
            var fileReference = new FileReference(imageFileName, relativePath, true);
            AddFile(fileReference);

            return fileReference;
        }
    }
}