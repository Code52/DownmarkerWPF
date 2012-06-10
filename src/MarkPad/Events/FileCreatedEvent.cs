namespace MarkPad.Events
{
    public class FileCreatedEvent
    {
        public FileCreatedEvent(string fullPath)
        {
            FullPath = fullPath;
        }

        public string FullPath { get; private set; }
    }
}