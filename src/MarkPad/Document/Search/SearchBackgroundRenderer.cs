using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace MarkPad.Document.Search
{
    public class SearchBackgroundRenderer : IBackgroundRenderer
    {
        public SearchBackgroundRenderer()
        {
            SearchHitsSegments = new TextSegmentCollection<TextSegment>();
        }

        public TextSegmentCollection<TextSegment> SearchHitsSegments { get; private set; }

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
            foreach (TextSegment current in this.SearchHitsSegments)
            {
                foreach (Rect current2 in BackgroundGeometryBuilder.GetRectsForSegment(textView, current))
                {
                    Point bottomLeft = current2.BottomLeft;
                    Point bottomRight = current2.BottomRight;
                    Pen pen = new Pen(new SolidColorBrush(Colors.OrangeRed), 1);
                    pen.Freeze();
                    double num = 2.5;
                    int count = System.Math.Max((int)((bottomRight.X - bottomLeft.X) / num) + 1, 4);
                    StreamGeometry streamGeometry = new StreamGeometry();
                    using (StreamGeometryContext streamGeometryContext = streamGeometry.Open())
                    {
                        streamGeometryContext.BeginFigure(bottomLeft, true, true);
                        streamGeometryContext.LineTo(current2.TopLeft, true, false);
                        streamGeometryContext.LineTo(current2.TopRight, true, false);
                        streamGeometryContext.LineTo(current2.BottomRight, true, false);
                    }
                    streamGeometry.Freeze();
                    drawingContext.DrawGeometry(Brushes.Transparent, pen, streamGeometry);
                }
            }
        }

        public KnownLayer Layer
        {
            get { return KnownLayer.Selection; }
        }
    }
}