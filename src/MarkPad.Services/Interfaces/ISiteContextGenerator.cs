namespace MarkPad.Services.Interfaces
{
    public interface ISiteContextGenerator
    {
        ISiteContext GetContext(string filename);
    }
}