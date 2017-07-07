using System.Collections.Generic;

namespace Athena.PubSub
{
    public class EventData<TEvent>
    {
        public EventData(TEvent evnt, IDictionary<string, object> environment)
        {
            Event = evnt;
            Environment = environment;
        }

        public TEvent Event { get; }
        public IDictionary<string, object> Environment { get; }
    }
}