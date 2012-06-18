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

        public override void ActivateItem(IScreen item)
        {
            base.ActivateItem(item);
            CurrentDocument = (DocumentViewModel) item;
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

            //<PreviewControl:HtmlPreview Html="{Binding CurrentDocument.Render}"
            //                 Filename="{Binding CurrentDocument.FileName}"
            //                 Margin="10,0,10,10" 
            //                        HorizontalAlignment="Stretch"
                                    
            //                 BrowserFontSize="{Binding CurrentDocument.FontSize}"
            //                 ScrollPercentage="{Binding ScrollPercentage, ElementName=doc}" 
            //                 x:Name="htmlPreview">
            //</PreviewControl:HtmlPreview>

            NotifyOfPropertyChange(()=>CurrentDocument);
        }

        public DocumentViewModel CurrentDocument { get; private set; }
    }
}
