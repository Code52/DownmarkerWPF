using System;

namespace Analects.XAMLConverters
{
    public class ToLowerConverter : MarkupConverter
    {
        protected override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
                return ((string)value).ToLower(culture);

            return value;
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
