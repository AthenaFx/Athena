using System.Collections.Generic;
using Athena.Configuration;
using Athena.EventStore.Serialization;
using Athena.Processes;

namespace Athena.EventStore.Projections
{
    public static class ProjectionsBootstrapExtensions
    {
        //TODO:Expand with better config options
        public static AthenaBootstrapper UseEventStoreProjections(this AthenaBootstrapper bootstrapper,
            EventStoreConnectionString connectionString, IEnumerable<EventStoreProjection> projections,
            ProjectionsPositionHandler projectionsPositionHandler, EventSerializer serializer = null)
        {
            serializer = serializer ?? new JsonEventSerializer();

            return bootstrapper
                .UsingPlugin(new ProjectionsPlugin())
                .UseProcess(new RunProjections(projections, connectionString, projectionsPositionHandler, serializer));
        }
    }
}