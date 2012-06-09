using System.ComponentModel;

namespace MarkPad.DocumentSources
{
    public interface ISiteItem : INotifyPropertyChanged
    {
        string Name { get; }
        ISiteItem[] Children { get; }
    }
}