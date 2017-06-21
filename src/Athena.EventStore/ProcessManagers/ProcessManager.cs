using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.EventStore.Serialization;

namespace Athena.EventStore.ProcessManagers
{
    public interface ProcessManager
    {
        string Name { get; }
        Task Handle(DeSerializationResult evnt, IDictionary<string, object> environment);
        IEnumerable<string> GetInterestingStreams();
        IReadOnlyDictionary<Type, string> GetEventMappings();
    }
}