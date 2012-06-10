namespace MarkPad.DocumentSources
{
    public interface ISiteContextGenerator
    {
        ISiteContext GetContext(string filename);
    }
}