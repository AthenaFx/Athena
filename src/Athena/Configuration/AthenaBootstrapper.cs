using System.Threading.Tasks;

namespace Athena.Configuration
{
    public interface AthenaBootstrapper
    {
        string ApplicationName { get; }
        string Environment { get; }
        AthenaBootstrapper WithApplicationName(string name);
        PartConfiguration<TPlugin> UsingPlugin<TPlugin>(TPlugin plugin) where TPlugin : AthenaPlugin;
        BootstrapEventListenerSetup<TEvent> When<TEvent>() where TEvent : SetupEvent;
        Task<AthenaContext> Build();
    }
}