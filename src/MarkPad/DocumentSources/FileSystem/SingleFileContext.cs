using System.Collections.ObjectModel;
using System.IO;
using Caliburn.Micro;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources.FileSystem
{
    public class SingleFileContext : PropertyChangedBase, ISiteContext
    {
        readonly ObservableCollection<ISiteItem> items = new ObservableCollection<ISiteItem>();

        public SingleFileContext(string fileName)
        {
            WorkingDirectory = Path.GetDirectoryName(fileName);
        }

        public ObservableCollection<ISiteItem> Items { get { return items; } }
        public bool IsLoading { get { return false; } }

        public void OpenItem(ISiteItem selectedItem)
        {
        }

        public string WorkingDirectory { get; private set; }
    }
}