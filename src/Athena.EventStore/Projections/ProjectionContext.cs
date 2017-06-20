using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.EventStore.Serialization;

namespace Athena.EventStore.Projections
{
    public class ProjectionContext
    {
        public ProjectionContext(EventStoreProjection projection, IEnumerable<DeSerializationResult> events,
            Action<DeSerializationResult> handled)
        {
            Projection = projection;
            Events = events;
            Handled = handled;
        }

        public EventStoreProjection Projection { get; }
        public IEnumerable<DeSerializationResult> Events { get; }
        public Action<DeSerializationResult> Handled { get; }
    }
}