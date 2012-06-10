using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using MarkPad.Events;

namespace MarkPad.DocumentSources
{
    public class FileSystemSiteItem : SiteItemBase
    {
        string originalFilename;

        public FileSystemSiteItem(IEventAggregator eventAggregator, string filePath) : 
            base(eventAggregator)
        {
            Path = filePath;
            originalFilename = System.IO.Path.GetFileName(filePath);
            Name = originalFilename;

            if (File.Exists(filePath))
                Children = new ObservableCollection<SiteItemBase>();
            else
            {
                var siteItems = Directory.GetDirectories(filePath)
                    .Select(d=>ToFileSystemItem(d, eventAggregator))
                    .OrderBy(i => i.Name)
                    .Concat(Directory.GetFiles(filePath) //TODO Restrict to markdown files only?
                                .Select(d=>ToFileSystemItem(d, eventAggregator))
                                .OrderBy(i => i.Name));
                Children = new ObservableCollection<SiteItemBase>(siteItems);
            }
        }

        public string Path { get; private set; }

        static FileSystemSiteItem ToFileSystemItem(string path, IEventAggregator eventAggregator)
        {
            return new FileSystemSiteItem(eventAggregator, path);
        }

        public override void CommitRename()
        {
            var fileDir = System.IO.Path.GetDirectoryName(Path);
            var newFilename = System.IO.Path.Combine(fileDir, Name);

            try
            {
                File.Move(Path, newFilename);
                EventAggregator.Publish(new FileRenamedEvent(Path, newFilename));
                Path = newFilename;
                originalFilename = Name;
            }
            catch (Exception ex)
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
    }
}