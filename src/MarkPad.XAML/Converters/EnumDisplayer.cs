using System;

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
            value = v;
        }

        public DisplayStringAttribute()
        {
        }
    }
}
