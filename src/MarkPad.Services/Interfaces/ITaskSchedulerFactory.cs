using System.Threading.Tasks;

namespace MarkPad.Services.Interfaces
{
    public interface ITaskSchedulerFactory
    {
        TaskScheduler Current { get; }
        TaskScheduler FromCurrentSynchronisationContext();
    }
}