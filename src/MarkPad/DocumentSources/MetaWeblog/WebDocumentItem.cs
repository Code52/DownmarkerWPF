using Caliburn.Micro;
using MarkPad.DocumentSources.WebSources;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.MetaWeblog
{
    public class WebDocumentItem : SiteItemBase
    {
        readonly BlogSetting blog;
        readonly IWebDocumentService webDocumentService;
        readonly string id;

        public WebDocumentItem(
            IWebDocumentService webDocumentService,
            IEventAggregator eventAggregator, 
            string id, 
            string title, 
            BlogSetting blog) :
            base(eventAggregator)
        {
            this.webDocumentService = webDocumentService;
            this.id = id;
            this.blog = blog;
            Name = title;
        }

        public string Id
        {
            get { return id; }
        }

        public override void CommitRename()
        {
            //webDocumentService.SaveDocument(blog, post);
        }

        public override void UndoRename()
        {
            //Name = post.title;
        }

        public override void Delete()
        {
            //webDocumentService.DeleteDocument(blog, post);
        }
    }
}