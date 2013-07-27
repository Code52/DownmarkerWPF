using System.Collections.Generic;
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
            string path, string markdownContent, JekyllSiteContext siteContext,  IEnumerable<FileReference> associatedFiles,
            IDocumentFactory documentFactory, IEventAggregator eventAggregator, IDialogService dialogService, IFileSystem fileSystem)
            : base(path, markdownContent, siteContext, associatedFiles, documentFactory, eventAggregator, dialogService, fileSystem)
        {
            this.siteContext = siteContext;
        }

        public override FileReference SaveImage(Bitmap image)
        {
            var pathToFile = Path.GetDirectoryName(FileName).Replace(siteContext.SiteBasePath, string.Empty).Trim('\\', '/');
            if (pathToFile.StartsWith("_")) pathToFile = string.Empty;
            var directory = Path.Combine(siteContext.SiteBasePath, "img", pathToFile);
            var imageFileName = GetFileNameBasedOnTitle(Title, directory);

            using (var stream = new FileStream(imageFileName, FileMode.Create))
                image.Save(stream, ImageFormat.Png);

            var relativePath = ToRelativePath(siteContext.SiteBasePath, FileName, imageFileName)
                .TrimStart('\\', '/')
                .Replace('\\', '/');
            var fileReference = new FileReference(imageFileName, relativePath, true);
            AddFile(fileReference);

            return fileReference;
        }
    }
}