using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Caliburn.Micro;
using MarkPad.Events;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources.FileSystem
{
    public class FileSystemSiteItem : SiteItem, IHandle<FileRenamedEvent>, IHandle<FileCreatedEvent>, IHandle<FileDeletedEvent>
    {
        readonly IFileSystem fileSystem;
        string originalFileName;

        public FileSystemSiteItem(IEventAggregator eventAggregator, IFileSystem fileSystem, string filePath) : 
            base(eventAggregator)
        {
            this.fileSystem = fileSystem;
            Path = filePath;
            originalFileName = System.IO.Path.GetFileName(filePath);
            Name = originalFileName;

            if (fileSystem.File.Exists(filePath))
                Children = new ObservableCollection<ISiteItem>();
            else
            {
                var siteItems = fileSystem.Directory.GetDirectories(filePath)
                    .Select(d => new FileSystemSiteItem(eventAggregator, fileSystem, d))
                    .OrderBy(i => i.Name)
                    .Concat(fileSystem.Directory.GetFiles(filePath) //TODO Restrict to markdown files only?
                                .Select(d => new FileSystemSiteItem(eventAggregator, fileSystem, d))
                                .OrderBy(i => i.Name));

                Children = new ObservableCollection<ISiteItem>(siteItems);
            }
        }

        public string Path { get; private set; }

        public override void CommitRename()
        {
            var fileDir = System.IO.Path.GetDirectoryName(Path);
            var newFileName = System.IO.Path.Combine(fileDir, Name);

            try
            {
                fileSystem.File.Move(Path, newFileName);
            }
            catch (IOException)
            {
                Name = originalFileName;
                //TODO show error
            }
            IsRenaming = false;
        }

        public override void UndoRename()
        {
            Name = originalFileName;
            IsRenaming = false;
        }

        public override void Delete()
        {
            fileSystem.File.Delete(Path);
        }

        public void Handle(FileRenamedEvent message)
        {
            if (Path == message.OriginalFileName)
            {
                Path = message.NewFileName;
                Name = System.IO.Path.GetFileName(Path);
                originalFileName = Name;
            }
        }

        public void Handle(FileCreatedEvent message)
        {
            if (Path == System.IO.Path.GetDirectoryName(message.FullPath))
            {
                var newItem = new FileSystemSiteItem(EventAggregator, fileSystem, message.FullPath);
                for (int i = 0; i < Children.Count; i++)
                {
                    if (String.Compare(Children[i].Name, newItem.Name, StringComparison.Ordinal) < 0) continue;
                    Children.Insert(i, newItem);
                    break;
                }
            }
        }

        public void Handle(FileDeletedEvent message)
        {
            foreach (var siteItemBase in Children.OfType<FileSystemSiteItem>())
            {
                if (siteItemBase.Path == message.FileName)
                {
                    Children.Remove(siteItemBase);
                    break;
                }
            }
        }
    }
}