using MarkPad.Services.Interfaces;

namespace MarkPad.DocumentSources
{
    public interface ISiteContextGenerator
    {
        ISiteContext GetContext(string filename);
    }
}