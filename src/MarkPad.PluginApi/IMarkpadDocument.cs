using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;

namespace MarkPad.Plugins
{
	public interface IMarkpadDocument : INotifyPropertyChanged
	{
		string MarkdownContent { get; set; }
	    string Title { get; }
	    ISiteContext SiteContext { get; }
	    string SaveLocation { get; }
	    Task<IMarkpadDocument> Save();
	    Task<IMarkpadDocument> SaveAs();
	    Task<IMarkpadDocument> Publish();

        /// <summary>
        /// Saves the image to the file system
        /// </summary>
        /// <param name="image"></param>
        /// <returns>The relative path to the image</returns>
        string SaveImage(Bitmap image);

        string ConvertToAbsolutePaths(string htmlDocument);
	}
}
