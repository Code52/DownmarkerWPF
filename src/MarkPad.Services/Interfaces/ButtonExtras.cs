using Ookii.Dialogs.Wpf;

namespace MarkPad.Services.Interfaces
{
    public class ButtonExtras
    {
        public ButtonExtras(ButtonType buttonType, string text, string note)
        {
            ButtonType = buttonType;
            Text = text;
            Note = note;
        }

        public ButtonType ButtonType { get; private set; }
        public string Text { get; private set; }
        public string Note { get; private set; }
    }
}