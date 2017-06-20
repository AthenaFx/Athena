using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.EventStore.Serialization;

namespace Athena.EventStore.Projections
{
    public interface EventStoreProjection
    {
        string Name { get; }
        IEnumerable<string> GetStreamsToProjectFrom();
        Task Apply(DeSerializationResult evnt, IDictionary<string, object> environment);
    }
}