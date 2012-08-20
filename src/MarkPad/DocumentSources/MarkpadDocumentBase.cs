using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources
{
    public abstract class MarkpadDocumentBase : IMarkpadDocument
    {
        readonly IDocumentFactory documentFactory;

        protected MarkpadDocumentBase(
            string title, string content, 
            string saveLocation,
            IDocumentFactory documentFactory)
        {
            Title = title;
            MarkdownContent = content;
            SaveLocation = saveLocation;
            this.documentFactory = documentFactory;
        }

        public string MarkdownContent { get; set; }
        public string Title { get; protected set; }
        public string SaveLocation { get; protected set; }
        public virtual ISiteContext SiteContext { get; protected set; }

        protected IDocumentFactory DocumentFactory
        {
            get { return documentFactory; }
        }

        public abstract Task<IMarkpadDocument> Save();

        public virtual Task<IMarkpadDocument> SaveAs()
        {
            return documentFactory.SaveDocumentAs(this);
        }

        public virtual Task<IMarkpadDocument> Publish()
        {
            return documentFactory.PublishDocument(null, this);
        }

        public abstract string SaveImage(Bitmap bitmap);
        public abstract string ConvertToAbsolutePaths(string htmlDocument);
        public abstract bool IsSameItem(ISiteItem siteItem);

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}