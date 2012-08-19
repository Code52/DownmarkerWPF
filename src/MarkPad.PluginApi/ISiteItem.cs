using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MarkPad.Plugins
{
    public interface ISiteItem : INotifyPropertyChanged, IDisposable
    {
        string Name { get; set; }
        ObservableCollection<ISiteItem> Children { get; }
        bool Selected { get; set; }
        bool IsRenaming { get; set; }
        void CommitRename();
        void UndoRename();
        void Delete();
    }
}