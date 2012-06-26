//FocusExtension code came from http://stackoverflow.com/a/1356781/75963
//Thanks to http://stackoverflow.com/users/125351/anvaka

using System.Windows;

namespace MarkPad.Behaviors
{
    public static class FocusExtension
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused", typeof (bool), typeof (FocusExtension),
                new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));


        private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uie = (UIElement) d;
            if ((bool) e.NewValue)
            {
                uie.Focus(); // Don't care about false values.
            }
        }
    }
}
