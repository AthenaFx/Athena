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
            var settings = new ProcessManagersSettings();

            bootstrapper = bootstrapper
                .UsingPlugin(new ProcessManagerPlugin())
                .UseProcess(new RunProcessManagers(settings));
            
            return new PartConfiguration<ProcessManagersSettings>(bootstrapper, settings);
        }
        
        public static PartConfiguration<ProcessManagersSettings> WithSerializer(
            this PartConfiguration<ProcessManagersSettings> config, EventSerializer serializer)
        {
            return config.ConfigurePart(x => x.WithSerializer(serializer));
        }

        public static PartConfiguration<ProcessManagersSettings> WithConnectionString(
            this PartConfiguration<ProcessManagersSettings> config, string connectionString)
        {
            return config.ConfigurePart(x => x.WithConnectionString(connectionString));
        }

        public static PartConfiguration<ProcessManagersSettings> WithProcessManager(
            this PartConfiguration<ProcessManagersSettings> config, ProcessManager processManager)
        {
            return config.ConfigurePart(x => x.WithProcessManager(processManager));
        }
    }
}