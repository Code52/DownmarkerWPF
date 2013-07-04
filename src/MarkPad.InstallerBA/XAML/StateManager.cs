using System;
using System.Windows;

namespace MarkPad.InstallerBA.XAML
{
    public class StateManager : DependencyObject
    {
        public static string GetVisualStateProperty(DependencyObject obj)
        {
            return (string)obj.GetValue(VisualStatePropertyProperty);
        }

        public static void SetVisualStateProperty(DependencyObject obj, string value)
        {
            obj.SetValue(VisualStatePropertyProperty, value);
        }

        public static readonly DependencyProperty VisualStatePropertyProperty =
            DependencyProperty.RegisterAttached(
                "VisualStateProperty",
                typeof(string),
                typeof(StateManager),
                new PropertyMetadata((s, e) =>
                {
                    var propertyName = (string)e.NewValue;

                    var ctrl = s as FrameworkElement;
                    if (ctrl == null)
                        throw new InvalidOperationException("This attached property only supports types derived from FrameworkElement.");

                    VisualStateManager.GoToElementState(ctrl, propertyName, true);
                }));
    }
}