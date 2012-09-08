using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using MarkPad.Plugins;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.GitHub
{
    public class GithubSiteContext : ISiteContext
    {
        readonly string workingDirectory;

        public GithubSiteContext(BlogSetting blog)
        {
            workingDirectory = Path.Combine(Path.GetTempPath(), blog.BlogName);
        }

        public ObservableCollection<ISiteItem> Items
        {
            get
            {
                return new ObservableCollection<ISiteItem>();
            }
        }

        public bool IsLoading { get; private set; }

        public string WorkingDirectory { get { return workingDirectory; } }

        public void OpenItem(ISiteItem selectedItem)
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}