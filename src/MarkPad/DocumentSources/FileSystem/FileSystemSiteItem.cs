using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Caliburn.Micro;
using MarkPad.Events;

namespace MarkPad.DocumentSources.FileSystem
{
    public class FileSystemSiteItem : SiteItemBase, IHandle<FileRenamedEvent>, IHandle<FileCreatedEvent>
    {
        readonly IFileSystem fileSystem;
        string originalFilename;

        public FileSystemSiteItem(IEventAggregator eventAggregator, IFileSystem fileSystem, string filePath) : 
            base(eventAggregator)
        {
            this.fileSystem = fileSystem;
            Path = filePath;
            originalFilename = System.IO.Path.GetFileName(filePath);
            Name = originalFilename;

            if (fileSystem.File.Exists(filePath))
                Children = new ObservableCollection<SiteItemBase>();
            else
            {
                var siteItems = fileSystem.Directory.GetDirectories(filePath)
                    .Select(d => new FileSystemSiteItem(eventAggregator, fileSystem, d))
                    .OrderBy(i => i.Name)
                    .Concat(fileSystem.Directory.GetFiles(filePath) //TODO Restrict to markdown files only?
                                .Select(d => new FileSystemSiteItem(eventAggregator, fileSystem, d))
                                .OrderBy(i => i.Name));

                Children = new ObservableCollection<SiteItemBase>(siteItems);
            }
        }

        public string Path { get; private set; }

        public override void CommitRename()
        {
            var fileDir = System.IO.Path.GetDirectoryName(Path);
            var newFilename = System.IO.Path.Combine(fileDir, Name);

            try
            {
                fileSystem.File.Move(Path, newFilename);
            }
            catch (IOException ex)
            {
                Name = originalFilename;
                //TODO show error
            }
            IsRenaming = false;
        }

        public override void UndoRename()
        {
            Name = originalFilename;
            IsRenaming = false;
        }

        public void Handle(FileRenamedEvent message)
        {
            Path = message.NewFilename;
            Name = System.IO.Path.GetFileName(Path);
            originalFilename = Name;
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
    }
}