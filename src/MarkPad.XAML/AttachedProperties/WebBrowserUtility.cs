using System.Windows;
using System.Windows.Controls;
using Awesomium.Windows.Controls;

namespace MarkPad.XAML.AttachedProperties
{
    public static class WebBrowserUtility
    {
        public static readonly DependencyProperty BindableContentProperty =
            DependencyProperty.RegisterAttached(
                "BindableContent",
                typeof(string),
                typeof(WebBrowserUtility),
                new UIPropertyMetadata(null, BindableContentPropertyChanged));

        public static string GetBindableContent(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableContentProperty);
        }

        public static void SetBindableContent(DependencyObject obj, string value)
        {
            obj.SetValue(BindableContentProperty, value);
        }

        public static void BindableContentPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            WebControl browser = o as WebControl;
            if (browser == null)
                return;

            string content = e.NewValue as string;
            if (string.IsNullOrEmpty(content))
                content = " ";

            browser.LoadHTML(content);
        }

    }
}
