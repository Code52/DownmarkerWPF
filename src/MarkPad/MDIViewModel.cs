using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Caliburn.Micro;
using MarkPad.Document;
using MarkPad.Preview;

namespace MarkPad
{
    public class MdiViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public HtmlPreview HtmlPreview;

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
            if (HtmlPreview == null)
            {
                var view = (MdiView)GetView();
                HtmlPreview = new HtmlPreview
                {
                    Margin = new Thickness(10, 0, 10, 10),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };
                HtmlPreview.SetBinding(HtmlPreview.HtmlProperty, new Binding("CurrentDocument.Render"));
                HtmlPreview.SetBinding(HtmlPreview.FileNameProperty, new Binding("CurrentDocument.MarkpadDocument.Title"));
                //HtmlPreview.SetBinding(HtmlPreview.BrowserFontSizeProperty, new Binding("CurrentDocument.FontSize"));
                HtmlPreview.SetBinding(HtmlPreview.ScrollPercentageProperty, new Binding("CurrentDocument.View.ScrollPercentage"));

                view.previewHost.Child = HtmlPreview;
            }

            NotifyOfPropertyChange(() => CurrentDocument);
        }

        public DocumentViewModel CurrentDocument { get; private set; }

        public void TitleMouseDown(IScreen activeItem, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
            {
                DeactivateItem(activeItem, true);
            }
        }
    }
}
