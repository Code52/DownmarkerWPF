using System;
using System.Threading.Tasks;

namespace MarkPad.Helpers
{
    public static class TaskExtensions
    {
        public static void PropagateExceptions(this Task task)
        {
            if (task == null)
                throw new ArgumentNullException("task");
            if (!task.IsCompleted)
                throw new InvalidOperationException("The task has not completed yet.");

            if (task.IsFaulted)
                task.Wait();
        }

        public static string GetErrorMessage(this AggregateException ex)
        {
            return ex.Flatten().InnerException.Message;
        }
    }
}
