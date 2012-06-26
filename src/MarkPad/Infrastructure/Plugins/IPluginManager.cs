using System.ComponentModel.Composition.Hosting;

namespace MarkPad.Infrastructure.Plugins
{
    public interface IPluginManager
    {
        CompositionContainer Container { get; }
    }
}