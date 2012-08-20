using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MarkPad.Plugins
{
    public interface ISiteContext : INotifyPropertyChanged
    {
        ObservableCollection<ISiteItem> Items { get; }
        bool IsLoading { get; }
        void OpenItem(ISiteItem selectedItem);
    }
}