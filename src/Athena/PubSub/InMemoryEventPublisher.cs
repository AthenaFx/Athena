using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.PubSub
{
    public class InMemoryEventPublisher : EventPublisher
    {
        private static readonly IDictionary<string, EventSubscription> Subscriptions 
            = new ConcurrentDictionary<string, EventSubscription>();
        
        public async Task Publish(object evnt)
        {
            foreach (var type in evnt.GetType().GetParentTypesFor())
            {
                var subscriptions = Subscriptions.Where(x => x.Value.SubscribedTo == type).ToList();

                await Task.WhenAll(subscriptions.Select(x => x.Value.Handle(evnt))).ConfigureAwait(false);
            }
        }

        public EventSubscription Subscribe<TEvent>(Func<TEvent, Task> subscription, string id)
        {
            var type = typeof(TEvent);

            var newSubscription = new EventSubscription(evnt => subscription((TEvent) evnt), 
                UnSubscribe, id, type);

            Subscriptions[id] = newSubscription;

            return newSubscription;
        }

        public void UnSubscribe(string id)
        {
            if (Subscriptions.ContainsKey(id))
                Subscriptions.Remove(id);
        }
    }
}