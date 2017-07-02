using System.Threading.Tasks;
using Athena.Configuration;
using Athena.Processes;

namespace Athena.EventStore.ProcessManagers
{
    public static class ProcessManagersBoostrapExtensions
    {
        public static PartConfiguration<ProcessManagersSettings> UseEventStoreProcesManagers(
            this AthenaBootstrapper bootstrapper)
        {
            return bootstrapper
                .UseProcess(new RunProcessManagers())
                .ConfigureWith<ProcessManagersSettings>((config, context) =>
                {
                    context.DefineApplication(config.Name, config.GetApplicationBuilder());
                    
                    return Task.CompletedTask;
                });
        }
    }
}