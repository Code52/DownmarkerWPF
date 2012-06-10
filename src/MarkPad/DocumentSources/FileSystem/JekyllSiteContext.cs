using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using Caliburn.Micro;
using MarkPad.Events;
using MarkPad.Infrastructure.Abstractions;
using MarkPad.Infrastructure.DialogService;
using MarkPad.PreviewControl;

namespace MarkPad.DocumentSources.FileSystem
{
    public class JekyllSiteContext : PropertyChangedBase, ISiteContext, IDisposable
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

            if (!Directory.Exists(absoluteImagePath))
                Directory.CreateDirectory(absoluteImagePath);

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filenameWithPath);
            if (fileNameWithoutExtension == null)
                return null;

            var filename = fileNameWithoutExtension
                .Replace(" ", string.Empty)
                .Replace(".", string.Empty);
            var count = 1;
            var imageFilename = filename + ".png";

            while (File.Exists(Path.Combine(absoluteImagePath, imageFilename)))
            {
                imageFilename = string.Format("{0}{1}.png", filename, count++);
            }

            using (var stream = new FileStream(Path.Combine(absoluteImagePath, imageFilename), FileMode.Create))
            {
                image.Save(stream, ImageFormat.Png);
            }

            var enumerable = imageFilename.Replace(basePath, string.Empty).TrimStart('\\', '/') //Get rid of starting /
                .Where(c => c == '/' || c == '\\') // select each / or \
                .Select(c => "..") // turn each into a ..
                .Concat(new[] { "img", imageFilename }); // concat with the image filename
            var relativePath = string.Join("\\", enumerable); //now we join with path separator giving relative path

            return relativePath;
        }

        public string ConvertToAbsolutePaths(string htmlDocument)
        {
            var matches = Regex.Matches(htmlDocument, "src=\"(?<url>(?<!http://).*?)\"", RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var replace = match.Captures[0].Value;
                var url = match.Groups["url"].Value;

                if (url.StartsWith("http://"))
                    continue;

                var filePath = Path.Combine(basePath, url.TrimStart('/'));
                if (!File.Exists(filePath))
                    continue;
                var base64String = Convert.ToBase64String(File.ReadAllBytes(filePath));
                htmlDocument = htmlDocument.Replace(replace, string.Format("src=\"data:image/png;base64,{0}\"", base64String));
            }

            return htmlDocument;
        }

        public ObservableCollection<SiteItemBase> Items
        {
            get { return items ?? (items = new FileSystemSiteItem(eventAggregator, fileSystem, basePath).Children); }
        }

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

        public void Dispose()
        {
            fileSystemWatcher.Dispose();
            foreach (var item in Items)
            {
                item.Dispose();
            }
        }
    }
}