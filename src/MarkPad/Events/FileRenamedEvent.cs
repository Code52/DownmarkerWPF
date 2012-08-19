namespace MarkPad.Events
{
    public class FileRenamedEvent
    {
        public FileRenamedEvent(string originalFileName, string newFileName)
        {
            OriginalFileName = originalFileName;
            NewFileName = newFileName;
        }

        public string OriginalFileName { get; private set; }
        public string NewFileName { get; private set; }
    }
}