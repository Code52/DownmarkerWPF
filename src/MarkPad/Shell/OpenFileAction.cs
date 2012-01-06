using Caliburn.Micro;
using MarkPad.Events;
using MarkPad.Services.Services;

namespace MarkPad.Shell
{
    public class OpenFileAction : IOpenFileAction
    {
        private readonly IEventAggregator eventAggregator;

        public OpenFileAction(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public void OpenFile(string path)
        {
            eventAggregator.Publish(new FileOpenEvent(path));
        }
    }
}