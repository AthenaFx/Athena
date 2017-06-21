using System.Collections.Generic;
using Athena.EventStore.Serialization;

namespace Athena.EventStore.ProcessManagers
{
    public class EventProcessingContext<TState>
    {
        public EventProcessingContext(DeSerializationResult evnt, IDictionary<string, object> environment, TState state)
        {
            Event = evnt;
            Environment = environment;
            State = state;
        }

        public DeSerializationResult Event { get; }
        public IDictionary<string, object> Environment { get; }
        public TState State { get; }
    }
}