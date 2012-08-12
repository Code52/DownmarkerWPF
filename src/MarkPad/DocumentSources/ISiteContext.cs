using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;

namespace MarkPad.DocumentSources
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

        ObservableCollection<SiteItemBase> Items { get; }
        bool IsLoading { get; }
        bool SupportsSave { get; }

        void OpenItem(SiteItemBase selectedItem);
        bool IsCurrentItem(SiteItemBase siteItemBase);
        bool Save(string displayName, string content);
    }
}