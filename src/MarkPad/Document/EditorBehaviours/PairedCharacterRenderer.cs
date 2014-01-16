using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace MarkPad.Document.EditorBehaviours
{
    public class PairedCharacterRenderer : IBackgroundRenderer
    {
        private Brush highlightBrush;
        private Pen highlightPen;

        public TextSegmentCollection<TextSegment> PairedCharacters { get; private set; }

        public PairedCharacterRenderer()
        {
            highlightBrush = new SolidColorBrush(Colors.LightBlue);
            highlightPen = new Pen(highlightBrush, 1.0);

            PairedCharacters = new TextSegmentCollection<TextSegment>();
        }

        private IEnumerable<Point> CreatePoints(Point start, Point end, double offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new Point(start.X + (double)i * offset, start.Y - (((i + 1) % 2 == 0) ? offset : 0.0));
            }
            yield break;
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            foreach (TextSegment character in this.PairedCharacters)
            {
                foreach (Rect characterRect in BackgroundGeometryBuilder.GetRectsForSegment(textView, character))
                {
                    drawingContext.DrawRectangle(highlightBrush, highlightPen, characterRect); 
                }
            }
        }

        public KnownLayer Layer
        {
            get { return KnownLayer.Selection; }
        }
    }
}
