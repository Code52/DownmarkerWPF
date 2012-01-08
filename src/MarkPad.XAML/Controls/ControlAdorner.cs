using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MarkPad.XAML.Controls
{
    public class ControlAdorner : Adorner
    {
        private FrameworkElement _child;
        private Point _adornerOffset;

        public ControlAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException();
            return _child;
        }

        public FrameworkElement Child
        {
            get { return _child; }
            set
            {
                if (_child != null)
                {
                    RemoveVisualChild(_child);
                }
                _child = value;
                if (_child != null)
                {
                    AddVisualChild(_child);
                }
            }
        }

        public Point Offset
        {
            get { return _adornerOffset; }
            set
            {
                _adornerOffset = value;

                //AdornerLayer adornerLayer = (AdornerLayer)this.Parent;
                //if (adornerLayer != null)
                //{
                //    adornerLayer.Update(this.AdornedElement);
                //}
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (_child == null)
                return constraint;

            _child.Measure(constraint);
            return _child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_child == null)
                return finalSize;

            _child.Arrange(new Rect(new Point(0, 0), finalSize));
            return new Size(_child.ActualWidth, _child.ActualHeight);
        }

        //public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        //{
        //    return base.GetDesiredTransform(transform);

        //    //GeneralTransformGroup newTransform = new GeneralTransformGroup();
        //    //newTransform.Children.Add(base.GetDesiredTransform(transform));
        //    //newTransform.Children.Add(new TranslateTransform(this._adornerOffset.X, this._adornerOffset.Y));
        //    //return newTransform;
        //}
    }
}
