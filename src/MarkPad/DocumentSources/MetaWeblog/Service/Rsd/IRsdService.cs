using System.Threading.Tasks;

namespace MarkPad.DocumentSources.MetaWeblog.Service.Rsd
{
    public interface IRsdService
    {
        Task<DiscoveryResult> DiscoverAddress(string webAPI);
    }
}