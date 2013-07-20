using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using MarkPad.Events;
using MarkPad.Infrastructure;
using MarkPad.Infrastructure.Abstractions;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources.FileSystem
{
    public class JekyllSiteContext : PropertyChangedBase, ISiteContext, IHandle<FileDeletedEvent>, IDisposable
    {
        readonly string siteBasePath;
        readonly IEventAggregator eventAggregator;
        readonly IDialogService dialogService;
        readonly IFileSystemWatcher fileSystemWatcher;
        readonly IFileSystem fileSystem;
        ObservableCollection<ISiteItem> items;

        public JekyllSiteContext(
            IEventAggregator eventAggregator, 
            IDialogService dialogService, 
            IFileSystem fileSystem, 
            IFileSystemWatcherFactory fileSystemWatcherFactory,
            string siteBasePath)
        {
            this.siteBasePath = siteBasePath;
            this.fileSystem = fileSystem;
            this.dialogService = dialogService;
            this.eventAggregator = eventAggregator;
            fileSystemWatcher = fileSystemWatcherFactory.Create(siteBasePath);
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

        public ObservableCollection<ISiteItem> Items
        {
            get { return items ?? (items = new FileSystemSiteItem(eventAggregator, fileSystem, SiteBasePath).Children); }
        }

        public bool IsLoading { get { return false; } }

        public string SiteBasePath
        {
            get { return siteBasePath; }
        }

        public void OpenItem(ISiteItem selectedItem)
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

        public string WorkingDirectory { get { return siteBasePath; } }

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