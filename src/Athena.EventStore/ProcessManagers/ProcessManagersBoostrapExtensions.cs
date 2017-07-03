using System.Threading.Tasks;
using Athena.Configuration;
using Athena.Logging;
using Athena.Processes;

namespace Athena.EventStore.ProcessManagers
{
    public static class ProcessManagersBoostrapExtensions
    {
        public static PartConfiguration<ProcessManagersSettings> UseEventStoreProcesManagers(
            this AthenaBootstrapper bootstrapper)
        {
            Logger.Write(LogLevel.Debug, $"Enabling process managers for {bootstrapper.ApplicationName}");
            
            return bootstrapper
                .UseProcess(new RunProcessManagers())
                .ConfigureWith<ProcessManagersSettings>((config, context) =>
                {
                    Logger.Write(LogLevel.Debug,
                        $"Configuring process managers (\"{config.Name}\") for {context.ApplicationName}");
                    
                    context.DefineApplication(config.Name, config.GetApplicationBuilder());
                    
                    return Task.CompletedTask;
                });
        }
    }
}