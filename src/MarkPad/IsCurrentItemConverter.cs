using System;
using System.Globalization;
using System.Windows.Data;
using MarkPad.Document;
using MarkPad.DocumentSources;

namespace MarkPad
{
    public class IsCurrentItemConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var documentViewModel = values[0] as DocumentViewModel;

            if (documentViewModel != null && documentViewModel.SiteContext != null)
                return documentViewModel.SiteContext.IsCurrentItem(values[1] as SiteItemBase);

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}