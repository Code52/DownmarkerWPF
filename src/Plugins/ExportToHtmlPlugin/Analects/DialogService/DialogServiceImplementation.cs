using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Ookii.Dialogs.Wpf;

namespace Analects.DialogService
{
    enum DialogMessageResult
    {
        None,
        OK,
        Cancel,
        Retry,
        Yes,
        No,
        Close,
        CustomButtonClicked,
    }

    [Flags]
    enum DialogMessageButtons
    {
        None = 0x0000,
        Ok = 0x0001,
        Yes = 0x0002,
        No = 0x0004,
        Cancel = 0x0008,
        Retry = 0x0010,
        Close = 0x0020
    }

    enum DialogMessageIcon
    {
        None,
        Error,
        Question,
        Warning,
        Information,
        Shield
    }

    internal class DialogServiceImplementation
    {
        private Window owner;

        public DialogServiceImplementation(Window owner)
        {
            this.owner = owner;
        }

        private DialogMessageResult DoOokiiMsgBox()
        {
            TaskDialog td = new TaskDialog();

            if ((Buttons & DialogMessageButtons.Ok) != 0) td.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
            if ((Buttons & DialogMessageButtons.Cancel) != 0) td.Buttons.Add(new TaskDialogButton(ButtonType.Cancel));

            if ((Buttons & DialogMessageButtons.Yes) != 0) td.Buttons.Add(new TaskDialogButton(ButtonType.Yes));
            if ((Buttons & DialogMessageButtons.No) != 0) td.Buttons.Add(new TaskDialogButton(ButtonType.No));

            if ((Buttons & DialogMessageButtons.Close) != 0) td.Buttons.Add(new TaskDialogButton(ButtonType.Close));
            if ((Buttons & DialogMessageButtons.Retry) != 0) td.Buttons.Add(new TaskDialogButton(ButtonType.Retry));

            switch (Icon)
            {
                case DialogMessageIcon.Error:
                    td.MainIcon = TaskDialogIcon.Error;
                    break;
                case DialogMessageIcon.Question:
                    td.MainIcon = TaskDialogIcon.Warning;
                    break;
                case DialogMessageIcon.Warning:
                    td.MainIcon = TaskDialogIcon.Warning;
                    break;
                case DialogMessageIcon.Information:
                    td.MainIcon = TaskDialogIcon.Information;
                    break;
                case DialogMessageIcon.Shield:
                    td.MainIcon = TaskDialogIcon.Shield;
                    break;
            }

            td.WindowTitle = Title;
            td.MainInstruction = Text;
            td.Content = Extra;

            var translation = new Dictionary<TaskDialogButton, ButtonType>();

            if (ButtonExtras != null && ButtonExtras.Any())
            {
                td.ButtonStyle = TaskDialogButtonStyle.CommandLinks;

                var buttonSet = td.Buttons.ToArray();
                td.Buttons.Clear();

                foreach (var extra in ButtonExtras)
                {
                    foreach (var button in buttonSet.Where(b => b.ButtonType == extra.ButtonType))
                    {
                        button.ButtonType = ButtonType.Custom;
                        button.Text = extra.Text;
                        button.CommandLinkNote = extra.Note;

                        translation.Add(button, extra.ButtonType);
                        td.Buttons.Add(button);
                    }
                }

                foreach (var button in buttonSet.Where(b => b.ButtonType != ButtonType.Custom))
                {
                    td.Buttons.Add(button);
                }
            }

            TaskDialogButton result = null;

            if (owner == null)
                result = td.ShowDialog();
            else
            {
                var dispatcher = owner.Dispatcher;

                result = (TaskDialogButton)dispatcher.Invoke(
                    new Func<TaskDialogButton>(() => td.ShowDialog(owner)),
                    System.Windows.Threading.DispatcherPriority.Normal);
            }

            var resultButtonType = result.ButtonType;
            if (resultButtonType == ButtonType.Custom)
                resultButtonType = translation[result];

            switch (resultButtonType)
            {
                case ButtonType.Cancel:
                    return DialogMessageResult.Cancel;
                case ButtonType.Close:
                    return DialogMessageResult.Close;
                case ButtonType.No:
                    return DialogMessageResult.No;
                case ButtonType.Ok:
                    return DialogMessageResult.OK;
                case ButtonType.Retry:
                    return DialogMessageResult.Retry;
                case ButtonType.Yes:
                    return DialogMessageResult.Yes;
            }

            return DialogMessageResult.None;
        }

        private DialogMessageResult DoWin32MsgBox()
        {
            MessageBoxButton button = MessageBoxButton.OK;
            if (Buttons == (DialogMessageButtons.Ok | DialogMessageButtons.Cancel))
                button = MessageBoxButton.OKCancel;
            else if (Buttons == (DialogMessageButtons.Yes | DialogMessageButtons.No))
                button = MessageBoxButton.YesNo;
            else if (Buttons == (DialogMessageButtons.Yes | DialogMessageButtons.No | DialogMessageButtons.Cancel))
                button = MessageBoxButton.YesNoCancel;

            MessageBoxImage icon = MessageBoxImage.None;
            switch (Icon)
            {
                case DialogMessageIcon.Error:
                    icon = MessageBoxImage.Error;
                    break;
                case DialogMessageIcon.Question:
                    icon = MessageBoxImage.Question;
                    break;
                case DialogMessageIcon.Warning:
                case DialogMessageIcon.Shield:
                    icon = MessageBoxImage.Warning;
                    break;
                case DialogMessageIcon.Information:
                    icon = MessageBoxImage.Information;
                    break;
            }

            MessageBoxResult result = MessageBoxResult.None;

            if (owner == null)
                result = MessageBox.Show(string.Format("{0}{1}{1}{2}", Text, Environment.NewLine, Extra), Title, button, icon);
            else
            {
                var dispatcher = owner.Dispatcher;

                result = (MessageBoxResult)dispatcher.Invoke(
                    new Func<MessageBoxResult>(() => MessageBox.Show(owner, string.Format("{0}{1}{1}{2}", Text, Environment.NewLine, Extra), Title, button, icon)),
                    System.Windows.Threading.DispatcherPriority.Normal);
            }

            switch (result)
            {
                case MessageBoxResult.Cancel:
                    return DialogMessageResult.Cancel;
                case MessageBoxResult.No:
                    return DialogMessageResult.No;
                case MessageBoxResult.None:
                    return DialogMessageResult.None;
                case MessageBoxResult.OK:
                    return DialogMessageResult.OK;
                case MessageBoxResult.Yes:
                    return DialogMessageResult.Yes;
            }

            return DialogMessageResult.None;
        }

        public string Title { get; set; }
        public string Extra { get; set; }
        public string Text { get; set; }
        public DialogMessageButtons Buttons { get; set; }
        public DialogMessageIcon Icon { get; set; }
        public ButtonExtras[] ButtonExtras { get; set; }

        public DialogMessageResult Show()
        {
            if (TaskDialog.OSSupportsTaskDialogs)
            {
                return DoOokiiMsgBox();
            }

            return DoWin32MsgBox();
        }
    }
}
