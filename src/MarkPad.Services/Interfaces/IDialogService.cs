using MarkPad.Services.Implementation;

namespace MarkPad.Services.Interfaces
{
    public interface IDialogService
    {
        bool ShowConfirmation(string title, string text, string extra, params ButtonExtras[] buttonExtras);
        bool? ShowConfirmationWithCancel(string title, string text, string extra, params ButtonExtras[] buttonExtras);

        void ShowMessage(string title, string text, string extra, params ButtonExtras[] buttonExtras);
        void ShowWarning(string title, string text, string extra, params ButtonExtras[] buttonExtras);
        void ShowError(string title, string text, string extra, params ButtonExtras[] buttonExtras);

        string[] GetFileOpenPath(string title, string filter);
        string GetFileSavePath(string title, string defaultExt, string filter);
    }
}
