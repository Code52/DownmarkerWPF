using Caliburn.Micro;
using MarkPad.DocumentSources;

namespace MarkPad.Tests.DocumentSources
{
    public class TestItem : SiteItem
    {
        public TestItem(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public override void CommitRename()
        {
                
        }

        public override void UndoRename()
        {
        
        }

        public override void Delete()
        {
            
        }

        public override void Dispose()
        {
            base.Dispose();
            Disposed = true;
        }

        public bool Disposed { get; set; }
    }
}