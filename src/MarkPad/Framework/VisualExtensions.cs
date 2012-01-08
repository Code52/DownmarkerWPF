using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MarkPad.Framework
{
    public static class VisualExtensions
    {
        public static T FindVisualChild<T>(this DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        public static double DistanceFromPoint(this FrameworkElement visual, Point point, UIElement pointIsRelativeTo)
        {
            var relativeVisualPosition = visual.TranslatePoint(new Point(0, 0), pointIsRelativeTo);
            var rectangle = new Rect(0, 0, visual.ActualWidth, visual.ActualHeight);
            rectangle.Offset(relativeVisualPosition.X, relativeVisualPosition.Y);

            if (rectangle.Contains(point))
            {
                return 0;
            }

            var distances = new[]
            {
                LineToPointDistance2D(rectangle.TopLeft, rectangle.TopRight, point, true),
                LineToPointDistance2D(rectangle.TopRight, rectangle.BottomRight, point, true),
                LineToPointDistance2D(rectangle.BottomRight, rectangle.BottomLeft, point, true),
                LineToPointDistance2D(rectangle.BottomLeft, rectangle.TopLeft, point, true)
            };

            return distances.Min();
        }

        //Compute the dot product AB . AC
        static double DotProduct(Point pointA, Point pointB, Point pointC)
        {
            double[] AB = new double[2];
            double[] BC = new double[2];
            AB[0] = pointB.X - pointA.X;
            AB[1] = pointB.Y - pointA.Y;
            BC[0] = pointC.X - pointB.X;
            BC[1] = pointC.Y - pointB.Y;
            double dot = AB[0] * BC[0] + AB[1] * BC[1];

            return dot;
        }

        //Compute the cross product AB x AC
        static double CrossProduct(Point pointA, Point pointB, Point pointC)
        {
            double[] AB = new double[2];
            double[] AC = new double[2];
            AB[0] = pointB.X - pointA.X;
            AB[1] = pointB.Y - pointA.Y;
            AC[0] = pointC.X - pointA.X;
            AC[1] = pointC.Y - pointA.Y;
            double cross = AB[0] * AC[1] - AB[1] * AC[0];

            return cross;
        }

        //Compute the distance from A to B
        static double Distance(Point pointA, Point pointB)
        {
            double d1 = pointA.X - pointB.X;
            double d2 = pointA.Y - pointB.Y;

            return Math.Sqrt(d1 * d1 + d2 * d2);
        }

        //Compute the distance from AB to C
        //if isSegment is true, AB is a segment, not a line.
        static double LineToPointDistance2D(Point pointA, Point pointB, Point pointC, bool isSegment)
        {
            double dist = CrossProduct(pointA, pointB, pointC) / Distance(pointA, pointB);
            if (isSegment)
            {
                double dot1 = DotProduct(pointA, pointB, pointC);
                if (dot1 > 0)
                    return Distance(pointB, pointC);

                double dot2 = DotProduct(pointB, pointA, pointC);
                if (dot2 > 0)
                    return Distance(pointA, pointC);
            }
            return Math.Abs(dist);
        }
    }
}
