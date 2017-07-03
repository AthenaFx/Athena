using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Logging;

namespace Athena.PubSub
{
    public class InMemoryEventPublisher : EventPublisher
    {
        private static readonly IDictionary<string, EventSubscription> Subscriptions 
            = new ConcurrentDictionary<string, EventSubscription>();
        
        public async Task Publish(object evnt, SettingsContext context)
        {
            foreach (var type in evnt.GetType().GetParentTypesFor())
            {
                var subscriptions = Subscriptions.Where(x => x.Value.SubscribedTo == type).ToList();

                await Task.WhenAll(subscriptions.Select(x => x.Value.Handle(evnt, context))).ConfigureAwait(false);
            }
        }

        public EventSubscription Subscribe<TEvent>(Func<TEvent, SettingsContext, Task> subscription, string id)
        {
            var type = typeof(TEvent);

            var newSubscription = new EventSubscription((evnt, context) => subscription((TEvent) evnt, context), 
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