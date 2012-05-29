using Ookii.Dialogs.Wpf;

namespace Analects.DialogService
{
    public class DialogService : IDialogService
    {
        #region IDialogService Members

        public bool ShowConfirmation(string title, string text, string extra, params ButtonExtras[] buttonExtras)
        {
            DialogServiceImplementation service = new DialogServiceImplementation(null);
            service.Icon = DialogMessageIcon.Question;
            service.Buttons = DialogMessageButtons.Yes | DialogMessageButtons.No;
            service.Title = title;
            service.Text = text;
            service.Extra = extra;
            service.ButtonExtras = buttonExtras;
            var result = service.Show();
            return result == DialogMessageResult.Yes;
        }

        public bool? ShowConfirmationWithCancel(string title, string text, string extra, params ButtonExtras[] buttonExtras)
        {
            DialogServiceImplementation service = new DialogServiceImplementation(null);
            service.Icon = DialogMessageIcon.Question;
            service.Buttons = DialogMessageButtons.Yes | DialogMessageButtons.No | DialogMessageButtons.Cancel;
            service.Title = title;
            service.Text = text;
            service.Extra = extra;
            service.ButtonExtras = buttonExtras;
            var result = service.Show();
            switch (result)
            {
                case DialogMessageResult.Yes:
                    return true;
                case DialogMessageResult.No:
                    return false;
            }
            return null;
        }

        public void ShowMessage(string title, string text, string extra, params ButtonExtras[] buttonExtras)
        {
            DialogServiceImplementation service = new DialogServiceImplementation(null);
            service.Icon = DialogMessageIcon.Information;
            service.Buttons = DialogMessageButtons.Ok;
            service.Title = title;
            service.Text = text;
            service.Extra = extra;
            service.ButtonExtras = buttonExtras;
            var result = service.Show();
        }

        public void ShowWarning(string title, string text, string extra, params ButtonExtras[] buttonExtras)
        {
            DialogServiceImplementation service = new DialogServiceImplementation(null);
            service.Icon = DialogMessageIcon.Warning;
            service.Buttons = DialogMessageButtons.Ok;
            service.Title = title;
            service.Text = text;
            service.Extra = extra;
            service.ButtonExtras = buttonExtras;
            var result = service.Show();
        }

        public void ShowError(string title, string text, string extra, params ButtonExtras[] buttonExtras)
        {
            DialogServiceImplementation service = new DialogServiceImplementation(null);
            service.Icon = DialogMessageIcon.Error;
            service.Buttons = DialogMessageButtons.Ok;
            service.Title = title;
            service.Text = text;
            service.Extra = extra;
            service.ButtonExtras = buttonExtras;
            var result = service.Show();
        }

        public string GetFileOpenPath(string title, string filter)
        {
            if (VistaOpenFileDialog.IsVistaFileDialogSupported)
            {
                VistaOpenFileDialog openFileDialog = new VistaOpenFileDialog();
                openFileDialog.Title = title;
                openFileDialog.CheckFileExists = true;
                openFileDialog.RestoreDirectory = true;

                openFileDialog.Filter = filter;

                if (openFileDialog.ShowDialog() == true)
                    return openFileDialog.FileName;
            }
            else
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Title = title;
                ofd.CheckFileExists = true;
                ofd.RestoreDirectory = true;

                ofd.Filter = filter;

                if (ofd.ShowDialog() == true)
                    return ofd.FileName;
            }

            return "";
        }

        public string GetFileSavePath(string title, string defaultExt, string filter)
        {
            if (VistaSaveFileDialog.IsVistaFileDialogSupported)
            {
                VistaSaveFileDialog saveFileDialog = new VistaSaveFileDialog();
                saveFileDialog.Title = title;
                saveFileDialog.DefaultExt = defaultExt;
                saveFileDialog.CheckFileExists = false;
                saveFileDialog.RestoreDirectory = true;

                saveFileDialog.Filter = filter;

                if (saveFileDialog.ShowDialog() == true)
                    return saveFileDialog.FileName;
            }
            else
            {
                Microsoft.Win32.SaveFileDialog ofd = new Microsoft.Win32.SaveFileDialog();
                ofd.Title = title;
                ofd.DefaultExt = defaultExt;
                ofd.CheckFileExists = false;
                ofd.RestoreDirectory = true;

                ofd.Filter = filter;

                if (ofd.ShowDialog() == true)
                    return ofd.FileName;
            }

            return "";
        }

        #endregion
    }
}
