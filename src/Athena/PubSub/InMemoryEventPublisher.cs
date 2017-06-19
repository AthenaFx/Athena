using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena.PubSub
{
    public class InMemoryEventPublisher : EventPublisher
    {
        private static readonly IDictionary<string, EventSubscription> Subscriptions 
            = new ConcurrentDictionary<string, EventSubscription>();
        
        public async Task Publish<TEvent>(TEvent evnt)
        {
            foreach (var type in GetTypesFrom(typeof(TEvent)))
            {
                var subscriptions = Subscriptions.Where(x => x.Value.SubscribedTo == type).ToList();

                await Task.WhenAll(subscriptions.Select(x => x.Value.Handle(evnt)));
            }
        }

        public Task<EventSubscription> Subscribe<TEvent>(Func<TEvent, Task> subscription, string id = null)
        {
            var type = typeof(TEvent);
            
            if(string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString();
            
            var newSubscription = new EventSubscription(x => subscription((TEvent) x), UnSubscribe, id, type);

            Subscriptions[id] = newSubscription;

            return Task.FromResult(newSubscription);
        }

        public void UnSubscribe(string id)
        {
            if (Subscriptions.ContainsKey(id))
                Subscriptions.Remove(id);
        }
        
        private static IEnumerable<Type> GetTypesFrom(Type eventType)
        {
            if (eventType == null)
                yield break;

            yield return eventType;

            var baseType = eventType.GetTypeInfo().BaseType;

            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.GetTypeInfo().BaseType;
            }

            foreach (var @interface in eventType.GetInterfaces())
                yield return @interface;
        }
    }
}