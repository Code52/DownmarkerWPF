using System.Threading.Tasks;

namespace MarkPad.Infrastructure.Abstractions
{
    public interface ITaskSchedulerFactory
    {
        TaskScheduler Current { get; }
        TaskScheduler FromCurrentSynchronisationContext();
    }
}