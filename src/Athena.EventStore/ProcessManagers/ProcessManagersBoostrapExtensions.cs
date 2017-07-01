using System;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.EventStore.Serialization;
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
                    context.DefineApplication("esprocessmanager", config.GetBuilder());
                    
                    return Task.CompletedTask;
                });
        }
        
        public static PartConfiguration<ProcessManagersSettings> WithSerializer(
            this PartConfiguration<ProcessManagersSettings> config, EventSerializer serializer)
        {
            return config.UpdateSettings(x => x.WithSerializer(serializer));
        }

        public static PartConfiguration<ProcessManagersSettings> WithConnectionString(
            this PartConfiguration<ProcessManagersSettings> config, string connectionString)
        {
            return config.UpdateSettings(x => x.WithConnectionString(connectionString));
        }

        public static PartConfiguration<ProcessManagersSettings> WithProcessManager(
            this PartConfiguration<ProcessManagersSettings> config, ProcessManager processManager)
        {
            return config.UpdateSettings(x => x.WithProcessManager(processManager));
        }
        
        public static PartConfiguration<ProcessManagersSettings> ConfigureApplication(
            this PartConfiguration<ProcessManagersSettings> config, 
            Func<AppFunctionBuilder, AppFunctionBuilder> configure)
        {
            return config.UpdateSettings(x => x.ConfigureApplication(configure));
        }
        
        public static PartConfiguration<ProcessManagersSettings> LoadStateWith(
            this PartConfiguration<ProcessManagersSettings> config, 
            Func<ProcessManagersSettings, ProcessStateLoader> getStateLoader)
        {
            return config.UpdateSettings(x => x.LoadStateWith(getStateLoader));
        }
    }
}