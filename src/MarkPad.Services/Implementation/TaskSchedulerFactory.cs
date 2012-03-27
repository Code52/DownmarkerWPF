using System.Threading.Tasks;
using MarkPad.Services.Interfaces;

namespace MarkPad.Services.Implementation
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