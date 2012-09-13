namespace MarkPad.DocumentSources.GitHub
{
    public class GitCommit
    {
        public string[] parents { get; set; }
        public string tree { get; set; }
        public string message { get; set; }
        public string sha { get; set; }
    }
}