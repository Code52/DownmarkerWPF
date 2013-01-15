using System.Collections.ObjectModel;
using Caliburn.Micro;
using MarkPad.Infrastructure;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources.NewDocument
{
    public class NewDocumentContext : PropertyChangedBase, ISiteContext
    {
        readonly string tempPath;

        public NewDocumentContext(IFileSystem fileSystem)
        {
            tempPath = fileSystem.GetTempPath();
            Items = new ObservableCollection<ISiteItem>();
        }

        public ObservableCollection<ISiteItem> Items { get; private set; }

        public bool IsLoading { get { return false; } }

        public void OpenItem(ISiteItem selectedItem)
        {
        }

        public string WorkingDirectory { get { return tempPath; } }
    }
}