using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Caliburn.Micro;
using MarkPad.Events;
using MarkPad.Infrastructure.Abstractions;
using MarkPad.Infrastructure.DialogService;
using MarkPad.PreviewControl;

namespace MarkPad.DocumentSources.FileSystem
{
    public class JekyllSiteContext : PropertyChangedBase, ISiteContext, IHandle<FileDeletedEvent>, IDisposable
    {
        readonly string basePath;
        readonly string filenameWithPath;
        readonly IEventAggregator eventAggregator;
        readonly IDialogService dialogService;
        readonly IFileSystemWatcher fileSystemWatcher;
        readonly IFileSystem fileSystem;
        ObservableCollection<SiteItemBase> items;

        public JekyllSiteContext(
            IEventAggregator eventAggregator, 
            IDialogService dialogService, 
            IFileSystem fileSystem, 
            IFileSystemWatcherFactory fileSystemWatcherFactory,
            string basePath, 
            string filename)
        {
            this.basePath = basePath;
            filenameWithPath = filename;
            this.fileSystem = fileSystem;
            this.dialogService = dialogService;
            this.eventAggregator = eventAggregator;
            fileSystemWatcher = fileSystemWatcherFactory.Create(basePath);
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.Created += FileSystemWatcherOnCreated;
            fileSystemWatcher.Changed += FileSystemWatcherOnChanged;
            fileSystemWatcher.Renamed += FileSystemWatcherOnRenamed;
            fileSystemWatcher.Deleted += FileSystemWatcherOnDeleted;

            fileSystemWatcher.EnableRaisingEvents = true;
        }

        void FileSystemWatcherOnDeleted(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            Debug.Write(string.Format("Deleted: {0} [{1}] (CT={2})",
                fileSystemEventArgs.Name,
                fileSystemEventArgs.FullPath,
                fileSystemEventArgs.ChangeType));

            eventAggregator.Publish(new FileDeletedEvent(fileSystemEventArgs.FullPath));
        }

        void FileSystemWatcherOnRenamed(object sender, RenamedEventArgs renamedEventArgs)
        {
            Debug.Write(string.Format("Renamed: {0} [{1}] from {2} [{3}]",
                renamedEventArgs.Name,
                renamedEventArgs.FullPath,
                renamedEventArgs.OldName,
                renamedEventArgs.OldFullPath));

            eventAggregator.Publish(new FileRenamedEvent(renamedEventArgs.OldFullPath, renamedEventArgs.FullPath));
        }

        void FileSystemWatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            Debug.Write(string.Format("Changed: {0} [{1}] (CT={2})",
                fileSystemEventArgs.Name,
                fileSystemEventArgs.FullPath,
                fileSystemEventArgs.ChangeType));
        }

        void FileSystemWatcherOnCreated(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            Debug.Write(string.Format("Created: {0} [{1}] (CT={2})",
                fileSystemEventArgs.Name,
                fileSystemEventArgs.FullPath,
                fileSystemEventArgs.ChangeType));

            eventAggregator.Publish(new FileCreatedEvent(fileSystemEventArgs.FullPath));
        }

        public string SaveImage(Bitmap image)
        {
            var absoluteImagePath = Path.Combine(basePath, "img");

            var imageFileName = SiteContextHelper.GetFileName(filenameWithPath, absoluteImagePath);

            using (var stream = new FileStream(Path.Combine(absoluteImagePath, imageFileName), FileMode.Create))
            {
                image.Save(stream, ImageFormat.Png);
            }

            //This basically will turn an absolute into a relative path in terms of the site context
            // So if site is c:\Site and img is at c:\Site\Folder\SubFolder\image.png
            // this will become ..\..\img\image.png
            var folderUp = imageFileName.Replace(basePath, string.Empty).TrimStart('\\', '/') //Get rid of starting /
                .Where(c => c == '/' || c == '\\') // select each / or \
                .Select(c => "..") // turn each into a ..
                .Concat(new[] { "img", imageFileName }); // concat with the image filename
            var relativePath = string.Join("\\", folderUp); //now we join with path separator giving relative path

            return relativePath;
        }

        public string ConvertToAbsolutePaths(string htmlDocument)
        {
            return SiteContextHelper.ConvertToAbsolutePaths(htmlDocument, basePath);
        }

        public ObservableCollection<SiteItemBase> Items
        {
            get { return items ?? (items = new FileSystemSiteItem(eventAggregator, fileSystem, basePath).Children); }
        }

        public bool IsLoading { get; private set; }
        public bool SupportsSave { get { return false; } }

        public void OpenItem(SiteItemBase selectedItem)
        {
            var fileItem = selectedItem as FileSystemSiteItem;
            if (fileItem == null || !File.Exists(fileItem.Path)) return;

            if (Constants.DefaultExtensions.Contains(Path.GetExtension(fileItem.Path).ToLower()))
            {
                eventAggregator.Publish(new FileOpenEvent(fileItem.Path));
            }
            else
            {
                try
                {
                    Process.Start(fileItem.Path);
                }
                catch (Exception ex)
                {
                    dialogService.ShowError("Failed to open file", "Cannot open {0}", ex.Message);
                }
            }
        }

        public bool IsCurrentItem(SiteItemBase siteItemBase)
        {
            var fileName = filenameWithPath.ToLower();
            var fileSystemSiteItem = (siteItemBase as FileSystemSiteItem);
            if (fileSystemSiteItem == null) return false;
            var path = fileSystemSiteItem.Path.ToLower();

            return fileName.StartsWith(path);
        }

        public bool Save(string displayName, string content)
        {
            return false;
        }

        public void Dispose()
        {
            fileSystemWatcher.Dispose();
            foreach (var item in Items)
            {
                item.Dispose();
            }
        }

        public void Handle(FileDeletedEvent message)
        {
            foreach (var siteItemBase in Items.OfType<FileSystemSiteItem>())
            {
                if (siteItemBase.Path == message.FileName)
                {
                    Items.Remove(siteItemBase);
                    break;
                }
            }
        }
    }
}