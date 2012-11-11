using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using MarkPad.Infrastructure;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources.NewDocument
{
    /// <summary>
    /// A new in memory document that has not been saved yet
    /// </summary>
    public class NewMarkpadDocument : MarkpadDocumentBase
    {
        readonly IFileSystem fileSystem;
        readonly List<FileReference> imagesToSave = new List<FileReference>();

        public NewMarkpadDocument(IFileSystem fileSystem, IDocumentFactory documentFactory, string content) : 
            base("New Document", content, null, new FileReference[0], documentFactory, new NewDocumentContext(fileSystem), fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public NewMarkpadDocument(IFileSystem fileSystem, IDocumentFactory documentFactory, string title, string content) :
            base(title, content, null, new FileReference[0], documentFactory, new NewDocumentContext(fileSystem), fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public override Task<IMarkpadDocument> Save()
        {
            return SaveAs();
        }

        public override async Task<IMarkpadDocument> SaveAs()
        {
            var newDocument = await base.SaveAs();
            foreach (var fileReference in imagesToSave)
            {
                newDocument.SaveImage(FileSystem.OpenBitmap(fileReference.FullPath));
            }

            return newDocument;
        }

        public override FileReference SaveImage(Bitmap image)
        {
            var imageFileName = GetFileNameBasedOnTitle(Title, SiteContext.WorkingDirectory);

            fileSystem.SaveImagePng(image, imageFileName);

            var relativePath = ToRelativePath(SiteContext.WorkingDirectory, SiteContext.WorkingDirectory, imageFileName);
            var fileReference = new FileReference(imageFileName, relativePath, false);
            imagesToSave.Add(fileReference);

            return fileReference;
        }

        public override string ConvertToAbsolutePaths(string htmlDocument)
        {
            return ConvertToAbsolutePaths(htmlDocument, fileSystem.GetTempPath());
        }

        public override bool IsSameItem(ISiteItem siteItem)
        {
            return false;
        }
    }
}