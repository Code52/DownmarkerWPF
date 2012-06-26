using Ookii.Dialogs.Wpf;

namespace MarkPad.Infrastructure.DialogService
{
    public class DialogService : IDialogService
    {
        public bool ShowConfirmation(string title, string text, string extra, params ButtonExtras[] buttonExtras)
        {
            var service = new DialogMessageService(null)
                          {
                              Icon = DialogMessageIcon.Question,
                              Buttons = DialogMessageButtons.Yes | DialogMessageButtons.No,
                              Title = title,
                              Text = text,
                              Extra = extra,
                              ButtonExtras = buttonExtras
                          };
            var result = service.Show();
            return result == DialogMessageResult.Yes;
        }

        public bool? ShowConfirmationWithCancel(string title, string text, string extra, params ButtonExtras[] buttonExtras)
        {
            var service = new DialogMessageService(null)
                          {
                              Icon = DialogMessageIcon.Question,
                              Buttons = DialogMessageButtons.Yes | DialogMessageButtons.No | DialogMessageButtons.Cancel,
                              Title = title,
                              Text = text,
                              Extra = extra,
                              ButtonExtras = buttonExtras
                          };
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
            var service = new DialogMessageService(null)
                                           {
                                               Icon = DialogMessageIcon.Information,
                                               Buttons = DialogMessageButtons.Ok,
                                               Title = title,
                                               Text = text,
                                               Extra = extra,
                                               ButtonExtras = buttonExtras
                                           };
            service.Show();
        }

        public void ShowWarning(string title, string text, string extra, params ButtonExtras[] buttonExtras)
        {
            var service = new DialogMessageService(null)
                                           {
                                               Icon = DialogMessageIcon.Warning,
                                               Buttons = DialogMessageButtons.Ok,
                                               Title = title,
                                               Text = text,
                                               Extra = extra,
                                               ButtonExtras = buttonExtras
                                           };
            service.Show();
        }

        public void ShowError(string title, string text, string extra, params ButtonExtras[] buttonExtras)
        {
            var service = new DialogMessageService(null)
                          {
                              Icon = DialogMessageIcon.Error,
                              Buttons = DialogMessageButtons.Ok,
                              Title = title,
                              Text = text,
                              Extra = extra,
                              ButtonExtras = buttonExtras
                          };
            service.Show();
        }

        public string[] GetFileOpenPath(string title, string filter)
        {
            if (VistaFileDialog.IsVistaFileDialogSupported)
            {
                var openFileDialog = new VistaOpenFileDialog
                                     {
                                         Title = title,
                                         CheckFileExists = true,
                                         RestoreDirectory = true,
                                         Multiselect = true,
                                         Filter = filter
                                     };

                if (openFileDialog.ShowDialog() == true)
                    return openFileDialog.FileNames;
            }
            else
            {
                var ofd = new Microsoft.Win32.OpenFileDialog
                          {
                              Title = title,
                              CheckFileExists = true,
                              RestoreDirectory = true,
                              Multiselect = true,
                              Filter = filter
                          };

                if (ofd.ShowDialog() == true)
                    return ofd.FileNames;
            }

            return null;
        }

        public string GetFileSavePath(string title, string defaultExt, string filter)
        {
            if (VistaFileDialog.IsVistaFileDialogSupported)
            {
                var saveFileDialog = new VistaSaveFileDialog
                                     {
                                         Title = title,
                                         DefaultExt = defaultExt,
                                         CheckFileExists = false,
                                         RestoreDirectory = true,
                                         Filter = filter
                                     };


                if (saveFileDialog.ShowDialog() == true)
                    return saveFileDialog.FileName;
            }
            else
            {
                var ofd = new Microsoft.Win32.SaveFileDialog
                          {
                              Title = title,
                              DefaultExt = defaultExt,
                              CheckFileExists = false,
                              RestoreDirectory = true,
                              Filter = filter
                          };
                
                if (ofd.ShowDialog() == true)
                    return ofd.FileName;
            }

            return "";
        }
    }
}
