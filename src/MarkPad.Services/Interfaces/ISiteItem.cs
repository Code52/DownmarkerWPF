using System.ComponentModel;

namespace MarkPad.Services.Interfaces
{
    public interface ISiteItem : INotifyPropertyChanged
    {
        string Name { get; }
        ISiteItem[] Children { get; }
    }
}