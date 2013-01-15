using System.Threading.Tasks;
using System.Windows.Input;

namespace MarkPad.Infrastructure
{
    public interface IAsyncCommand<in T> : ICommand, IRaiseCanExecuteChanged
    {
        Task ExecuteAsync(T obj);
    }
}