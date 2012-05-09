using System.Threading.Tasks;

namespace MarkPad.Services.Metaweblog.Rsd
{
    public interface IRsdService
    {
        Task<DiscoveryResult> DiscoverAddress(string webAPI);
    }
}