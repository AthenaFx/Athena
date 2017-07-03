using System.Threading.Tasks;
using Athena.Configuration;
using Athena.Logging;

namespace Athena.EventStore.ProcessManagers
{
    public static class ProcessManagersBoostrapExtensions
    {
        public static PartConfiguration<EventStoreProcessManagers> UseEventStoreProcesManagers(
            this AthenaBootstrapper bootstrapper)
        {
            Logger.Write(LogLevel.Debug, $"Enabling process managers for {bootstrapper.ApplicationName}");
            
            return bootstrapper
                .Part<EventStoreProcessManagers>()
                .OnSetup((config, context) =>
                {
                    Logger.Write(LogLevel.Debug,
                        $"Configuring process managers (\"{config.Name}\") for {context.ApplicationName}");
                    
                    context.DefineApplication(config.Name, config.GetApplicationBuilder());
                    
                    return Task.CompletedTask;
                })
                .OnStartup((processManagers, context) => processManagers.Start(context))
                .OnShutdown((processManagers, context) => processManagers.Stop());
        }
    }
}