using Caliburn.Micro;
using MarkPad.Events;
using MarkPad.Services.Services;

namespace MarkPad.Shell
{
    /// <summary>
    /// Logic for processing the "Open File" command
    /// </summary>
    public class OpenFileCommand : IOpenFileCommand
    {
        private readonly IEventAggregator eventAggregator;

        public OpenFileCommand(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public void OpenFile(string path)
        {
            eventAggregator.Publish(new FileOpenEvent(path));
        }
    }
}