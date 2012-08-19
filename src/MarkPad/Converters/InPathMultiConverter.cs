using System;
using System.Globalization;
using System.Windows.Data;

namespace MarkPad.Converters
{
    public class InPathMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var fileName = ((string) values[0]).ToLower();
            var path = ((string)values[1]).ToLower();

            return fileName.StartsWith(path);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}