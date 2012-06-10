namespace MarkPad.Events
{
    public class FileRenamedEvent
    {
        public FileRenamedEvent(string originalFilename, string newFilename)
        {
            OriginalFilename = originalFilename;
            NewFilename = newFilename;
        }

        public string OriginalFilename { get; private set; }
        public string NewFilename { get; private set; }
    }
}