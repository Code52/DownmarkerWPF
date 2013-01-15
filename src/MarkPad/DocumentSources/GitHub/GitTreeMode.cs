namespace MarkPad.DocumentSources.GitHub
{
    public enum GitTreeMode
    {
        File = 100644,
        Executable = 100755,
        SubDirectory = 040000
    }
}