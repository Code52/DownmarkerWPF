using System.Windows.Media.Imaging;

namespace MarkPad.Services.Interfaces
{
    public interface ISiteContext
    {
        /// <summary>
        /// Saves the image to the file system
        /// </summary>
        /// <param name="getImage"></param>
        /// <returns>The relative path to the image</returns>
        string SaveImage(BitmapSource getImage);

        string ConvertToAbsolutePaths(string htmlDocument);
    }
}