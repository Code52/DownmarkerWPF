using System;
using System.Windows;
using MahApps.Metro.Converters;

namespace MarkPad.Converters
{
    public class NullToVisibilityConverter : MarkupConverter
    {
        protected override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value != null) ? Visibility.Visible : Visibility.Collapsed;
        }
        protected override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
