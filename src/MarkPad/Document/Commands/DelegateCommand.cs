using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MarkPad.Document.Commands
{
    public class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private string displayName;
        Func<object, bool> canExecute;
        Action<object> execute;

        public DelegateCommand(string displayName, Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.displayName = displayName;
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }

        public override string ToString()
        {
            return displayName;
        }
    }
}
