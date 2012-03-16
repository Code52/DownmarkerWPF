using System.Drawing;

namespace MarkPad.Services.Interfaces
{
    public interface ISiteContext
    {
        /// <summary>
        /// Saves the image to the file system
        /// </summary>
        /// <param name="image"></param>
        /// <returns>The relative path to the image</returns>
        string SaveImage(Bitmap image);

        string ConvertToAbsolutePaths(string htmlDocument);
    }
}