using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using MarkPad.Document;
using MarkPad.PreviewControl;

namespace MarkPad
{
    internal class MDIViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public HtmlPreview htmlPreview;

        public void Open(IScreen screen)
        {
            ActivateItem(screen);
        }

        protected override IScreen DetermineNextItemToActivate(System.Collections.Generic.IList<IScreen> list, int lastIndex)
        {
            if (list.Count == 0)
                CurrentDocument = null;
            return base.DetermineNextItemToActivate(list, lastIndex);
        }

        protected override void ChangeActiveItem(IScreen newItem, bool closePrevious)
        {
            base.ChangeActiveItem(newItem, closePrevious);
            CurrentDocument = (DocumentViewModel) newItem;
            if (htmlPreview == null)
            {
                var view = (MDIView)GetView();
                htmlPreview = new HtmlPreview
                {
                    Margin = new Thickness(10, 0, 10, 10),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };
                htmlPreview.SetBinding(HtmlPreview.HtmlProperty, new Binding("CurrentDocument.Render"));
                htmlPreview.SetBinding(HtmlPreview.FilenameProperty, new Binding("CurrentDocument.FileName"));
                htmlPreview.SetBinding(HtmlPreview.BrowserFontSizeProperty, new Binding("CurrentDocument.FontSize"));
                htmlPreview.SetBinding(HtmlPreview.FilenameProperty, new Binding("CurrentDocument.FileName"));
                htmlPreview.SetBinding(HtmlPreview.ScrollPercentageProperty, new Binding("CurrentDocument.View.ScrollPercentage"));

                view.previewHost.Child = htmlPreview;
            }

            NotifyOfPropertyChange(() => CurrentDocument);
        }

        public DocumentViewModel CurrentDocument { get; private set; }
    }
}
