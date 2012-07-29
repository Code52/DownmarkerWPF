using System;
using System.Windows;
using MahApps.Metro.Converters;
using MarkPad.Document;

namespace MarkPad.Converters
{
    public class BoolToGridLengthConverter : MarkupConverter
    {
        private readonly GridLength zeroLength = new GridLength(0);

        public BoolToGridLengthConverter()
        {
            Length = new GridLength(1, GridUnitType.Star);
        }

        public bool Invert { get; set; }

        public GridLength Length { get; set; }

        protected override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var flag = false;
            if (value is bool)
            {
                flag = (bool)value;
            }

            return (flag ^ Invert) ? Length : zeroLength;
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = false;

            if (value is GridLength)
            {
                result = Math.Abs(((GridLength)value).Value - 0) > 0.1;
            }

            return result ^ Invert;
        }
    }

    public class DocumentToSiteConverter : MarkupConverter
    {
        private readonly GridLength zeroLength = new GridLength(0);

        protected override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var doc = (DocumentViewModel)value;
            if (doc.SiteContext == null)
                return zeroLength;
            else
                return new GridLength(125);
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
