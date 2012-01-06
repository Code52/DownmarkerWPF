namespace MarkPad.Framework.Events
{
    public class FileOpenEvent
    {
        public FileOpenEvent(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }
    }
}
