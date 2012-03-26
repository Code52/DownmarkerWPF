using System.Threading.Tasks;

namespace MarkPad.Services.Interfaces
{
    public class TaskSchedulerFactory : ITaskSchedulerFactory
    {
        public TaskScheduler Current
        {
            get { return TaskScheduler.FromCurrentSynchronizationContext(); }
        }
    }
}