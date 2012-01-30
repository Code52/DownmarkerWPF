using System;
using System.Windows;

namespace MarkPad.XAML.Converters
{
    public class BoolToGridLengthConverter : MarkupConverter
    {
        private readonly GridLength ZeroLength = new GridLength(0);

        public BoolToGridLengthConverter()
        {
            Length = new GridLength(1, GridUnitType.Star);
        }

        public bool Invert { get; set; }

        public GridLength Length { get; set; }

        protected override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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
            return (flag ^ Invert) ? Length : ZeroLength;
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = false;

            if (value is GridLength)
            {
                result = ((GridLength)value).Value != 0;
            }

            return result ^ Invert;
        }
    }
}
