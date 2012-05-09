using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using Analects.XAMLConverters;

namespace MarkPad.XAML.Converters
{
    public class EnumDisplayConverter : MarkupConverter
    {
        private static readonly Dictionary<object, string> displayValues = new Dictionary<object, string>();
        private static readonly Dictionary<string, object> reverseValues = new Dictionary<string, object>();

        private void GetDisplayString(object value)
        {
            var valueType = value.GetType();

            var fields = valueType.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                var a = (DisplayStringAttribute[]) field.GetCustomAttributes(typeof(DisplayStringAttribute), false);

                string displayString = GetDisplayStringValue(a, valueType);
                object enumValue = field.GetValue(null);

                if (displayString == null)
                {
                    displayString = enumValue.ToString();
                }
                if (displayString != null)
                {
                    displayValues.Add(enumValue, displayString);
                    reverseValues.Add(displayString, enumValue);
                }
            }
        }

        private string GetDisplayStringValue(DisplayStringAttribute[] a, Type type)
        {
            if (a == null || a.Length == 0) return null;
            DisplayStringAttribute dsa = a[0];
            if (!string.IsNullOrEmpty(dsa.ResourceKey))
            {
                ResourceManager rm = new ResourceManager(type);
                return rm.GetString(dsa.ResourceKey);
            }
            return dsa.Value;
        }

        protected override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!displayValues.ContainsKey(value))
                GetDisplayString(value);

            return displayValues[value];
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!reverseValues.ContainsKey(value.ToString()))
                return null;

            return reverseValues[value.ToString()];
        }
    }
}