using System;
using System.Threading.Tasks;

namespace Athena.Configuration
{
    public interface AthenaBootstrapper : SettingsContext
    {
        PartConfiguration<TPart> Configure<TPart>(string key = null) where TPart : class, new();
        
        PartConfiguration<TPart> ConfigureWith<TPart, TEvent>(Func<TPart, TEvent, AthenaSetupContext, Task> setup,
            Func<TEvent, bool> filter = null, string key = null) 
            where TPart : class, new() where TEvent : SetupEvent;

        AthenaBootstrapper ShutDownWith<TEvent>(Func<TEvent, AthenaContext, Task> shutDown,
            Func<TEvent, bool> filter = null)
            where TEvent : ShutdownEvent;
        
        Task<AthenaContext> Build();
    }
}