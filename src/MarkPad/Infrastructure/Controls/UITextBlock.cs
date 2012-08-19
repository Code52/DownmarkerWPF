using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;

namespace MarkPad.Infrastructure.Controls
{
    public class UITextBlock : TextBlock
    {
        private static readonly DependencyPropertyKey IsTextTrimmedKey = DependencyProperty.RegisterReadOnly(
            "IsTextTrimmed",
            typeof(bool),
            typeof(UITextBlock),
            new PropertyMetadata(false));

        private static readonly DependencyProperty IsTextTrimmedProperty = IsTextTrimmedKey.DependencyProperty;

        public UITextBlock()
        {
            DefaultStyleKey = typeof (UITextBlock);

            SizeChanged += UITextBlockSizeChanged;
            Loaded += AddValueChangedToTextProperty;
            Unloaded += RemoveValueChangedToTextProperty;
        }

        private void RemoveValueChangedToTextProperty(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource != this)
                return;

            var textDescriptor = DependencyPropertyDescriptor.FromProperty(TextProperty, typeof(UITextBlock));
            textDescriptor.RemoveValueChanged(this, TextChanged);
        }

        private void AddValueChangedToTextProperty(object sender, RoutedEventArgs e)
        {
            var textDescriptor = DependencyPropertyDescriptor.FromProperty(TextProperty, typeof(UITextBlock));
            textDescriptor.AddValueChanged(this, TextChanged);
        }

        public bool IsTextTrimmed
        {
            get { return GetIsTextTrimmed(this); }
            set { SetIsTextTrimmed(this, value); }
        }

        public static bool GetIsTextTrimmed(DependencyObject o)
        {
            return (bool)o.GetValue(IsTextTrimmedProperty);
        }

        private static void SetIsTextTrimmed(DependencyObject target, bool value)
        {
            target.SetValue(IsTextTrimmedKey, value);
        }

        private static void TextChanged(object sender, EventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (null == textBlock)
            {
                return;
            }

            SetIsTextTrimmed(textBlock, TextTrimming.None != textBlock.TextTrimming && CalculateIsTextTrimmed(textBlock));
        }

        static void UITextBlockSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (null == textBlock)
            {
                return;
            }

            SetIsTextTrimmed(textBlock, TextTrimming.None != textBlock.TextTrimming && CalculateIsTextTrimmed(textBlock));
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new UITextBlockAutomationPeer(this);
        }

        private static bool CalculateIsTextTrimmed(TextBlock textBlock)
        {
            var typeface = new Typeface(
                textBlock.FontFamily,
                textBlock.FontStyle,
                textBlock.FontWeight,
                textBlock.FontStretch);

            // FormattedText is used to measure the whole width of the text held up by TextBlock container
            var formattedText = new FormattedText(
                textBlock.Text,
                System.Threading.Thread.CurrentThread.CurrentCulture,
                textBlock.FlowDirection,
                typeface,
                textBlock.FontSize,
                textBlock.Foreground);
            
            return formattedText.Width > textBlock.ActualWidth ||
                formattedText.Height > textBlock.ActualHeight;
        }
    }
}
