using System;
using System.Globalization;
using System.Windows;

namespace Analects.XAMLConverters
{
    public class NullToVisibilityConverter : MarkupConverter
    {
        public bool Invert { get; set; }

        protected override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value != null;
            return (flag ^ Invert) ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
