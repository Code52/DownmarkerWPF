using System;

namespace MarkPad.Infrastructure
{
    public class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action executeMethod) : base(o=>executeMethod())
        {
        }

        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod) : base(o=>executeMethod(), o=>canExecuteMethod())
        {
        }
    }
}