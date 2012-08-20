using System;
using System.Globalization;
using System.Windows.Data;
using MarkPad.Document;
using MarkPad.Plugins;

namespace MarkPad.Converters
{
    public class IsCurrentItemConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var documentViewModel = values[0] as DocumentViewModel;

            if (documentViewModel != null && documentViewModel.MarkpadDocument.SiteContext != null)
                return documentViewModel.MarkpadDocument.IsSameItem(values[1] as ISiteItem);

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}