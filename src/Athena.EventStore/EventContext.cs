using System.Collections.Generic;
using EventStore.ClientAPI;

namespace Athena.EventStore
{
    public class EventContext<TState>
    {
        public EventContext(TState state, IDictionary<string, object> metaData, ResolvedEvent originalEvent)
        {
            MetaData = metaData;
            OriginalEvent = originalEvent;
            State = state;
        }

        public TState State { get; }
        public IDictionary<string, object> MetaData { get; }
        public ResolvedEvent OriginalEvent { get; }
    }
}