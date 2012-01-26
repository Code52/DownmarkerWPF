using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Resources;

namespace MarkPad.XAML.Converters
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DisplayStringAttribute : Attribute
    {
        private readonly string value;
        public string Value
        {
            get { return value; }
        }

        public string ResourceKey { get; set; }

        public DisplayStringAttribute(string v)
        {
            this.value = v;
        }

        public DisplayStringAttribute()
        {
        }
    }

    public class EnumDisplayConverter : MarkupConverter
    {
        private Type type;
        private IDictionary displayValues;
        private IDictionary reverseValues;

        public EnumDisplayConverter()
        {
        }

        public EnumDisplayConverter(Type type)
        {
            this.Type = type;
        }

        public Type Type
        {
            get { return type; }
            set
            {
                if (!value.IsEnum)
                    throw new ArgumentException("parameter is not an Enumermated type", "value");
                this.type = value;
            }
        }

        public ReadOnlyCollection<string> DisplayNames
        {
            get
            {
                Type displayValuesType = typeof(Dictionary<,>)
                                        .GetGenericTypeDefinition().MakeGenericType(typeof(string), type);
                this.displayValues = (IDictionary)Activator.CreateInstance(displayValuesType);

                this.reverseValues =
                   (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>)
                            .GetGenericTypeDefinition()
                            .MakeGenericType(type, typeof(string)));

                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    DisplayStringAttribute[] a = (DisplayStringAttribute[])
                                                field.GetCustomAttributes(typeof(DisplayStringAttribute), false);

                    string displayString = GetDisplayStringValue(a);
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
                return new List<string>((IEnumerable<string>)displayValues.Values).AsReadOnly();
            }
        }

        private string GetDisplayStringValue(DisplayStringAttribute[] a)
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
            return displayValues[value];
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return reverseValues[value];
        }
    }
}
