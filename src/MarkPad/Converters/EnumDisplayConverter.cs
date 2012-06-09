using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using MahApps.Metro.Converters;

namespace MarkPad.Converters
{
    public class EnumDisplayConverter : MarkupConverter
    {
        static readonly Dictionary<object, string> DisplayValues = new Dictionary<object, string>();
        static readonly Dictionary<string, object> ReverseValues = new Dictionary<string, object>();

        private void GetDisplayString(object value)
        {
            var valueType = value.GetType();

            var fields = valueType.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                var a = (DescriptionAttribute[]) field.GetCustomAttributes(typeof(DescriptionAttribute), false);

                var displayString = GetDisplayStringValue(a);
                var enumValue = field.GetValue(null);

                if (displayString == null)
                {
                    displayString = enumValue.ToString();
                }

                DisplayValues.Add(enumValue, displayString);
                ReverseValues.Add(displayString, enumValue);
            }
        }

        static string GetDisplayStringValue(ICollection<DescriptionAttribute> descriptionAttributes)
        {
            if (descriptionAttributes == null || descriptionAttributes.Count == 0) 
                return null;
            return descriptionAttributes.First().Description;
        }

        protected override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!DisplayValues.ContainsKey(value))
                GetDisplayString(value);

            return DisplayValues[value];
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!ReverseValues.ContainsKey(value.ToString()))
                return null;

            return ReverseValues[value.ToString()];
        }
    }
}