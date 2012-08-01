using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MarkPad.Helpers;

namespace MarkPad.Document.Controls
{
    public class FloatingToolBar : Popup
    {
        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(ICommandSource), typeof(FloatingToolBar), new UIPropertyMetadata(null));
        public static readonly DependencyProperty FadeAwayProperty = DependencyProperty.Register("FadeAway", typeof(bool), typeof(FloatingToolBar), new PropertyMetadata(true));
        private Window window;

        public FloatingToolBar()
        {
            AllowsTransparency = true;
            Loaded += ControlLoaded;
            Unloaded += ControlUnloaded;
            StaysOpen = true;
            FocusManager.SetIsFocusScope(this, true);
        }

        public ICommandSource CommandTarget
        {
            get { return (ICommandSource)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        public bool FadeAway
        {
            get { return (bool)GetValue(FadeAwayProperty); }
            set { SetValue(FadeAwayProperty, value); }
        }

        public FrameworkElement Content
        {
            get
            {
                var content = Child as FrameworkElement;
                if (content == null)
                {
                    throw new Exception("The FloatingToolBar requires a FrameworkElement to be its content");
                }

                return content;
            }
        }

        private void ControlLoaded(object sender, RoutedEventArgs e)
        {
            window = Window.GetWindow(this);
            Attach();
        }

        private void Attach()
        {
            if (PlacementTarget == null)
                return;

            PlacementTarget.LostFocus += Hide;

            if (window != null)
            {
                window.LocationChanged += LocationChanged;
                window.PreviewMouseMove += MouseMoved;
                window.Deactivated += WindowDeactivated;
            }
        }

        public void Hide()
        {
            Content.Opacity = 0;
            IsOpen = false;
        }

        public void Show()
        {
            Hide();

            Placement = PlacementMode.Mouse;

            UpdateOpacity();
            IsOpen = true;
            UpdateOpacity();
        }

		public void Show(UIElement target, Point point)
		{
			Hide();

			Placement = PlacementMode.Relative;
			PlacementTarget = target;
			HorizontalOffset = point.X;
			VerticalOffset = point.Y;
			
			UpdateOpacity();
			IsOpen = true;
			UpdateOpacity();
		}

        private void MouseMoved(object sender, MouseEventArgs e)
        {
            UpdateOpacity();
        }

        private void UpdateOpacity()
        {
            if (Content.IsMouseDirectlyOver)
            {
                Opacity = 1;
                return;
            }

            if(!FadeAway)
            {
                Content.Opacity = 1;
                return;
            }

            var position = Mouse.GetPosition(window);
            var distance = Content.DistanceFromPoint(position, window);

            if (distance < 2)
            {
                Content.Opacity = 1;
            }
            else if (distance > 30)
            {
                Content.Opacity = 0.2;
            }
            else
            {
                Content.Opacity = ((30 - distance) / 30.00) + 0.2;
            }

            if (Content.Opacity < 0.2)
            {
                Content.Opacity = 0.2;
            }
        }

        private void WindowDeactivated(object sender, EventArgs e)
        {
            Hide();
        }

        private void LocationChanged(object sender, EventArgs e)
        {
            if (IsOpen)
            {
                Hide();
            }
        }

        private void ControlUnloaded(object sender, RoutedEventArgs e)
        {
            Detach();
        }

        private void Hide(object sender, RoutedEventArgs e)
        {
            IsOpen = false;
        }

        private void Detach()
        {
            if (PlacementTarget == null)
                return;

            PlacementTarget.LostFocus -= Hide;

            if (window != null)
            {
                window.PreviewMouseMove -= MouseMoved;
                window.LocationChanged -= LocationChanged;
                window.Deactivated -= WindowDeactivated;
            }
        }
    }
}
