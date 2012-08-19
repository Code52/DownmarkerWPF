using System.Threading.Tasks;

namespace MarkPad.Infrastructure.Abstractions
{
    public class TaskSchedulerFactory : ITaskSchedulerFactory
    {
        public TaskScheduler Current
        {
            get { return TaskScheduler.Current; }
        }

        public TaskScheduler FromCurrentSynchronisationContext()
        {
            return TaskScheduler.FromCurrentSynchronizationContext();
        }
    }
}