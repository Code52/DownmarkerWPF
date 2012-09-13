namespace MarkPad.DocumentSources.GitHub
{
    public class GitFile
    {
        public int mode { get; set; }
        public string url { get; set; }
        public string path { get; set; }
        public string type { get; set; }
        public string sha { get; set; }
        public string content { get; set; }
    }
}