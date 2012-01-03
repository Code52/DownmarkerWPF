namespace MarkPad.Services.Interfaces
{
    public interface IDialogService
    {
        bool ShowConfirmation(string title, string text, string extra);
        bool? ShowConfirmationWithCancel(string title, string text, string extra);

        void ShowMessage(string title, string text, string extra);
        void ShowWarning(string title, string text, string extra);
        void ShowError(string title, string text, string extra);

        string GetFileOpenPath(string title, string filter);
        string GetFileSavePath(string title, string defaultExt, string filter);
    }
}
