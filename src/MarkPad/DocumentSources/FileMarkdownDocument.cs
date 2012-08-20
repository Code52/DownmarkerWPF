using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.DocumentSources.FileSystem;
using MarkPad.Events;
using MarkPad.Helpers;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources
{
    public class FileMarkdownDocument : MarkpadDocumentBase, IHandle<FileRenamedEvent>, IDisposable
    {
        readonly IEventAggregator eventAggregator;

        public FileMarkdownDocument(string path, string markdownContent, ISiteContext siteContext, IDocumentFactory documentFactory, IEventAggregator eventAggregator) : 
            base(Path.GetFileNameWithoutExtension(path), markdownContent, Path.GetDirectoryName(path), documentFactory)
        {
            FileName = path;
            this.eventAggregator = eventAggregator;
            SiteContext = siteContext;
            eventAggregator.Subscribe(this);
        }

        public string FileName { get; private set; }

        public override Task<IMarkpadDocument> Save()
        {
            var streamWriter = new StreamWriter(FileName);
            return streamWriter
                .WriteAsync(MarkdownContent)
                .ContinueWith<IMarkpadDocument>(t =>
                {
                    streamWriter.Dispose();

                    t.PropagateExceptions();

                    return this;
                });
        }

        public override Task<IMarkpadDocument> SaveAs()
        {
            return base.SaveAs()
                .ContinueWith(t=>
                {
                    var markpadDocument = (FileMarkdownDocument)t.Result;
                    markpadDocument.MoveImagesFolder(FileName, Title, markpadDocument.FileName);

                    return t.Result;
                });
        }

        public override string SaveImage(Bitmap image)
        {
            var directory = Path.GetDirectoryName(FileName);
            var absoluteImagePath = GetImageDirectory(directory, Title);
            if (!Directory.Exists(absoluteImagePath))
                Directory.CreateDirectory(absoluteImagePath);

            var imageFileName = SiteContextHelper.GetFileName(Title, absoluteImagePath);

            using (var stream = new FileStream(imageFileName, FileMode.Create))
            {
                image.Save(stream, ImageFormat.Png);
            }

            return SiteContextHelper.ToRelativePath(directory, FileName, imageFileName);
        }

        string GetImageDirectory(string directory, string title)
        {
            return Path.Combine(directory, title + "_images");
        }

        public override string ConvertToAbsolutePaths(string htmlDocument)
        {
            var imageDirectory = GetImageDirectory(Path.GetDirectoryName(FileName), Title);
            return SiteContextHelper.ConvertToAbsolutePaths(htmlDocument, imageDirectory);
        }

        public override bool IsSameItem(ISiteItem siteItem)
        {
            var fileItem = siteItem as FileSystemSiteItem;

            if (fileItem == null)
                return false;

            return fileItem.Path == FileName;
        }

        public void Handle(FileRenamedEvent message)
        {
            var originalFileName = message.OriginalFileName;
            var newFileName = message.NewFileName;
            if (FileName == originalFileName)
            {
                var oldTitle = Title;
                FileName = newFileName;
                Title = new FileInfo(FileName).Name;

                //Move any images
                MoveImagesFolder(originalFileName, oldTitle, newFileName);
            }
        }

        void MoveImagesFolder(string originalFileName, string oldTitle, string newFileName, bool copy = false)
        {
            var imageDirectory = GetImageDirectory(Path.GetDirectoryName(originalFileName), oldTitle);
            var newImageDirectory = GetImageDirectory(Path.GetDirectoryName(newFileName), Title);
            if (Directory.Exists(imageDirectory))
            {
                if (copy)
                    CopyDirectory(imageDirectory, newImageDirectory);
                else
                    Directory.Move(imageDirectory, newImageDirectory);
            }

            var oldRelativePath = SiteContextHelper.ToRelativePath(Path.GetDirectoryName(originalFileName), originalFileName,
                                                                   imageDirectory);
            var newRelativePath = SiteContextHelper.ToRelativePath(Path.GetDirectoryName(newFileName), newFileName,
                                                                   newImageDirectory);
            MarkdownContent = MarkdownContent
                .Replace(oldRelativePath, newRelativePath);
        }

        private static void CopyDirectory(string sourcePath, string destPath)
        {
            if (!Directory.Exists(destPath))
                Directory.CreateDirectory(destPath);

            foreach (var file in Directory.GetFiles(sourcePath))
            {
                var fileName = Path.GetFileName(file);
                if (fileName == null) continue;
                var dest = Path.Combine(destPath, fileName);
                File.Copy(file, dest);
            }

            foreach (var folder in Directory.GetDirectories(sourcePath))
            {
                var fileName = Path.GetFileName(folder);
                if (fileName == null) continue;
                var dest = Path.Combine(destPath, fileName);
                CopyDirectory(folder, dest);
            }
        }

        public void Dispose()
        {
            eventAggregator.Unsubscribe(this);
        }
    }
}