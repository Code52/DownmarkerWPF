using System;
using System.Globalization;
using System.Windows;

namespace Analects.XAMLConverters
{
    public class BoolToVisibilityConverter : MarkupConverter
    {
        public bool Invert { get; set; }

        protected override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool)
            {
                flag = (bool)value;
            }
            else
            {
                if (value is bool?)
                {
                    bool? flag2 = (bool?)value;
                    flag = (flag2.HasValue && flag2.Value);
                }
            }
            return (flag ^ Invert) ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            if (value is Visibility)
            {
                result = (Visibility)value == Visibility.Visible;
            }

            return result ^ Invert;
        }
    }
}
