using System.IO;
using System.Linq;
using Caliburn.Micro;

namespace MarkPad.Services.Interfaces
{
    public class FileSystemSiteItem : PropertyChangedBase, ISiteItem
    {
        public FileSystemSiteItem(string filePath)
        {
            Path = filePath;
            Name = System.IO.Path.GetFileName(filePath);

            if (File.Exists(filePath))
                Children = new ISiteItem[0];
            else
            {
                Children = Directory.GetDirectories(filePath)
                    .Select(ToFileSystemItem)
                    .OrderBy(i => i.Name)
                    .Concat(Directory.GetFiles(filePath) //TODO Restrict to markdown files only?
                                .Select(ToFileSystemItem)
                                .OrderBy(i => i.Name))
                    .Cast<ISiteItem>()
                    .ToArray();
            }
        }

        static FileSystemSiteItem ToFileSystemItem(string path)
        {
            return new FileSystemSiteItem(path);
        }

        public string Name { get; private set; }
        public string Path { get; private set; }
        public ISiteItem[] Children { get; private set; }
    }
}