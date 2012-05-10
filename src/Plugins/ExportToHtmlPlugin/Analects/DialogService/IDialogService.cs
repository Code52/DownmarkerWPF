using Ookii.Dialogs.Wpf;

namespace Analects.DialogService
{
    public class ButtonExtras
    {
        public ButtonExtras(ButtonType buttonType, string text, string note)
        {
            this.ButtonType = buttonType;
            this.Text = text;
            this.Note = note;
        }

        public ButtonType ButtonType { get; private set; }
        public string Text { get; private set; }
        public string Note { get; private set; }
    }

    public interface IDialogService
    {
        bool ShowConfirmation(string title, string text, string extra, params ButtonExtras[] buttonExtras);
        bool? ShowConfirmationWithCancel(string title, string text, string extra, params ButtonExtras[] buttonExtras);

        void ShowMessage(string title, string text, string extra, params ButtonExtras[] buttonExtras);
        void ShowWarning(string title, string text, string extra, params ButtonExtras[] buttonExtras);
        void ShowError(string title, string text, string extra, params ButtonExtras[] buttonExtras);

        string GetFileOpenPath(string title, string filter);
        string GetFileSavePath(string title, string defaultExt, string filter);
    }
}
