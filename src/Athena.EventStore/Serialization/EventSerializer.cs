using System;
using System.Collections.Generic;
using EventStore.ClientAPI;

namespace Athena.EventStore.Serialization
{
    public interface EventSerializer
    {
        SerializationResult Serialize(Guid eventId, object evnt, IDictionary<string, object> headers);
        DeSerializationResult DeSerialize(ResolvedEvent resolvedEvent);
    }
}