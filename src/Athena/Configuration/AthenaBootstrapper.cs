using System.Threading.Tasks;

namespace Athena.Configuration
{
    public interface AthenaBootstrapper
    {
        string ApplicationName { get; }
        AthenaBootstrapper WithApplicationName(string name);
        AthenaBootstrapper UsingPlugin<TPlugin>(TPlugin plugin) where TPlugin : AthenaPlugin;
        BootstrapEventListenerSetup<TEvent> When<TEvent>() where TEvent : SetupEvent;
        Task<AthenaContext> Build();
    }
}