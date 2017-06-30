using System.Collections.Generic;
using Athena.Configuration;
using Athena.EventStore.Serialization;
using Athena.Processes;

namespace Athena.EventStore.ProcessManagers
{
    public static class ProcessManagersBoostrapExtensions
    {
        //TODO:Expand with better config options
        public static AthenaBootstrapper UseEventStoreProcesManagers(this AthenaBootstrapper bootstrapper,
            EventStoreConnectionString connectionString, IEnumerable<ProcessManager> processManagers,
            EventSerializer eventSerializer = null)
        {
            eventSerializer = eventSerializer ?? new JsonEventSerializer();

            return bootstrapper
                .UsingPlugin(new ProcessManagerPlugin())
                .UseProcess(new RunProcessManagers(processManagers, connectionString, eventSerializer));
        }
    }
}