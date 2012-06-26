namespace MarkPad.Events
{
    public class FileDeletedEvent
    {
        public FileDeletedEvent(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; private set; }
    }
}