using System;
using System.Threading.Tasks;

namespace MarkPad.Infrastructure
{
    public class AwaitableDelegateCommand : AwaitableDelegateCommand<object>, IAsyncCommand
    {
        public AwaitableDelegateCommand(Func<Task> executeMethod)
            : base(o=>executeMethod())
        {
        }

        public AwaitableDelegateCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
            : base(o=>executeMethod(), o=>canExecuteMethod())
        {
        }
    }
}