using System.ComponentModel;
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
	}
}
