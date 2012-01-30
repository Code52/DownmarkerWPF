using System.Diagnostics;

namespace MarkPad.XAML.Converters
{
    public class DebugConverter : MarkupConverter
    {
        protected override object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debug.WriteLine("[DEBUGCONVERTER Convert] Value      = " + value);
            Debug.WriteLine("[DEBUGCONVERTER Convert] TargetType = " + targetType);
            Debug.WriteLine("[DEBUGCONVERTER Convert] Parameter  = " + parameter);
            Debug.WriteLine("[DEBUGCONVERTER Convert] Culture    = " + culture);

            return value;
        }

        protected override object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debug.WriteLine("[DEBUGCONVERTER ConvertBack] Value      = " + value);
            Debug.WriteLine("[DEBUGCONVERTER ConvertBack] TargetType = " + targetType);
            Debug.WriteLine("[DEBUGCONVERTER ConvertBack] Parameter  = " + parameter);
            Debug.WriteLine("[DEBUGCONVERTER ConvertBack] Culture    = " + culture);

            return value;
        }
    }
}
