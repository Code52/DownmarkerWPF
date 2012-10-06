using White.Core.UIItems;

namespace Markpad.UITests.Infrastructure
{
    public class MarkpadDocument : ScreenComponent<MarkpadWindow>
    {
        public MarkpadDocument(MarkpadWindow markpadWindow) : base(markpadWindow)
        {
            
        }

        public string Title
        {
            get
            {
                var label = Screen.WhiteWindow.Get<Label>("DocumentTitle");
                return label.Text;
            }
        }
    }
}