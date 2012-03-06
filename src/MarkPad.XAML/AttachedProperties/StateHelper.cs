using System;
using System.Windows;
using System.Windows.Controls;

namespace MarkPad.XAML.AttachedProperties
{
    public class VisualStateHelper : DependencyObject
    {
        public static string GetVisualStateName(DependencyObject target)
        {
            return (string)target.GetValue(VisualStateNameProperty);
        }

        public static void SetVisualStateName(DependencyObject target, string visualStateName)
        {
            target.SetValue(VisualStateNameProperty, visualStateName);
        }

        public static readonly DependencyProperty VisualStateNameProperty = DependencyProperty.RegisterAttached(
            "VisualStateName", typeof(string), typeof(VisualStateHelper), new PropertyMetadata(OnVisualStateNameChanged));

        private static void OnVisualStateNameChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            string visualStateName = (string)args.NewValue;
            var control = sender as Grid;
            if (control == null)
                throw new InvalidOperationException("This attached property only supports types derived from Control.");

            // Apply the visual state.
            var x = VisualStateManager.GoToState(control, visualStateName, true);
        }
    }
}