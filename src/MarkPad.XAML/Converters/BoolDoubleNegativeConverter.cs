using System;
using System.Globalization;
using System.Windows.Data;

namespace MarkPad.XAML.Converters
{
    public class BoolDoubleNegativeConverter : MarkupConverter
    {
        protected override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? 0 : double.NegativeInfinity;
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}