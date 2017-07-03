using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.EventStore.Serialization;

namespace Athena.EventStore.ProcessManagers
{
    public class EventProcessingContext<TState>
    {
        public EventProcessingContext(DeSerializationResult evnt, IDictionary<string, object> environment, TState state,
            Func<object, DateTime, Task> requestTimeout)
        {
            Event = evnt;
            Environment = environment;
            State = state;
            RequestTimeout = requestTimeout;
        }

        public DeSerializationResult Event { get; }
        public IDictionary<string, object> Environment { get; }
        public TState State { get; }
        public Func<object, DateTime, Task> RequestTimeout { get; }
    }
}