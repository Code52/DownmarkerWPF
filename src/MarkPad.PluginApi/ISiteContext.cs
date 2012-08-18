using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;

namespace MarkPad.Plugins
{
    public interface ISiteContext : INotifyPropertyChanged
    {
        /// <summary>
        /// Saves the image to the file system
        /// </summary>
        /// <param name="image"></param>
        /// <returns>The relative path to the image</returns>
        string SaveImage(Bitmap image);

        string ConvertToAbsolutePaths(string htmlDocument);

        ObservableCollection<ISiteItem> Items { get; }

        void OpenItem(ISiteItem selectedItem);
    }
}