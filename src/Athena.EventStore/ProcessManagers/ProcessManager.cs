using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.EventStore.Serialization;
using EventStore.ClientAPI;

namespace Athena.EventStore.ProcessManagers
{
    public interface ProcessManager
    {
        string Name { get; }

        Task Handle(DeSerializationResult evnt, IDictionary<string, object> environment, AthenaContext context,
            EventSerializer serializer, IEventStoreConnection connection);
        IEnumerable<string> GetInterestingStreams();
        IReadOnlyDictionary<Type, string> GetEventMappings();
    }
}