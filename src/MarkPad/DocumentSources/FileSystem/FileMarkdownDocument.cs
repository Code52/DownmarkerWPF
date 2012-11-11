using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.Events;
using MarkPad.Infrastructure;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Plugins;
using Ookii.Dialogs.Wpf;

namespace MarkPad.DocumentSources.FileSystem
{
    public class FileMarkdownDocument : MarkpadDocumentBase, IHandle<FileRenamedEvent>, IDisposable
    {
        readonly IEventAggregator eventAggregator;
        readonly IDialogService dialogService;

        public FileMarkdownDocument(
            string path, string markdownContent, ISiteContext siteContext, IEnumerable<FileReference> associatedFiles,
            IDocumentFactory documentFactory, IEventAggregator eventAggregator, IDialogService dialogService, IFileSystem fileSystem) : 
            base(Path.GetFileNameWithoutExtension(path), markdownContent, Path.GetDirectoryName(path), associatedFiles, documentFactory, siteContext, fileSystem)
        {
            FileName = path;
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            eventAggregator.Subscribe(this);
        }

        public string FileName { get; private set; }

        public override async Task<IMarkpadDocument> Save()
        {
            var fileInfo = FileSystem.FileInfo(FileName);
            if (fileInfo.IsReadOnly)
            {
                var result = dialogService.ShowConfirmationWithCancel(
                    "Markpad", string.Format("{0} is readonly, what do you want to do?", FileName),
                    null,
                    new ButtonExtras(ButtonType.Yes, "Make writable", "Marks the file as writable then saves"),
                    new ButtonExtras(ButtonType.No, "Save As", "Saves the file as another name"));

                //The dialog service returns null if cancelled, true is yes, false if no.... Go figure
                if (result == null)
                    throw new TaskCanceledException("Save cancelled");

                if (result == true)
                    fileInfo.IsReadOnly = false;
                else
                    return await SaveAs();
            }

            await FileSystem.File.WriteAllTextAsync(FileName, MarkdownContent);

            return this;
        }

        public override FileReference SaveImage(Bitmap image)
        {
            var directory = Path.GetDirectoryName(FileName);
            var absoluteImagePath = GetImageDirectory(directory, Title);

            if (!FileSystem.Directory.Exists(absoluteImagePath))
                FileSystem.Directory.CreateDirectory(absoluteImagePath);

            var imageFileName = GetFileNameBasedOnTitle(Title, absoluteImagePath);
            FileSystem.SaveImagePng(image, imageFileName);

            var relativePath = ToRelativePath(directory, FileName, imageFileName);
            var fileReference = new FileReference(imageFileName, relativePath, true);
            AddFile(fileReference);

            return fileReference;
        }

        string GetImageDirectory(string directory, string title)
        {
            return Path.Combine(directory, title + "_images");
        }

        public override string ConvertToAbsolutePaths(string htmlDocument)
        {
            return ConvertToAbsolutePaths(htmlDocument, Path.GetDirectoryName(FileName));
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
                Title = Path.GetFileNameWithoutExtension(FileName);

                //Move any images
                MoveImagesFolder(originalFileName, oldTitle, newFileName);
            }
        }

        void MoveImagesFolder(string originalFileName, string oldTitle, string newFileName)
        {
            var imageDirectory = GetImageDirectory(Path.GetDirectoryName(originalFileName), oldTitle);
            var newImageDirectory = GetImageDirectory(Path.GetDirectoryName(newFileName), Title);
            if (FileSystem.Directory.Exists(imageDirectory))
            {
                FileSystem.Directory.Move(imageDirectory, newImageDirectory);
            }

            var oldRelativePath = ToRelativePath(Path.GetDirectoryName(originalFileName), originalFileName, imageDirectory);
            var newRelativePath = ToRelativePath(Path.GetDirectoryName(newFileName), newFileName, newImageDirectory);
            MarkdownContent = MarkdownContent.Replace(oldRelativePath, newRelativePath);
        }

        public void Dispose()
        {
            eventAggregator.Unsubscribe(this);
        }
    }
}