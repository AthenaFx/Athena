using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena
{
    public class InMemoryEventBus : EventBus
    {
        private static readonly IDictionary<Type, IDictionary<Guid, InMemoryEventSubscription>> Subscriptions
            = new ConcurrentDictionary<Type, IDictionary<Guid, InMemoryEventSubscription>>();

        public Task Publish<TEvent>(TEvent evnt)
        {
            var eventTypes = FindInheritedEventTypes(evnt);

            var subscriptions = Subscriptions
                .Where(x => eventTypes.Contains(x.Key))
                .SelectMany(x => x.Value.Values)
                .ToList();

            return Task.WhenAll(subscriptions.Select(x => x.Handle(evnt)));
        }

        public EventSubscription Subscribe<TEvent>(Func<TEvent, Task> subscriber)
        {
            if(!Subscriptions.ContainsKey(typeof(TEvent)))
                Subscriptions[typeof(TEvent)] = new ConcurrentDictionary<Guid, InMemoryEventSubscription>();

            var id = Guid.NewGuid();

            var remove = (Action) (() =>
            {
                Subscriptions[typeof(TEvent)].Remove(id);
            });

            var handle = (Func<object, Task>) (evnt => subscriber((TEvent) evnt));

            var subscription = new InMemoryEventSubscription(handle, remove);

            Subscriptions[typeof(TEvent)][id] = subscription;

            return subscription;
        }

        private static IEnumerable<Type> FindInheritedEventTypes(object evnt)
        {
            yield return evnt.GetType();

            foreach (var @interface in evnt.GetType().GetInterfaces())
                yield return @interface;

            var baseType = evnt.GetType().GetTypeInfo().BaseType;

            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.GetTypeInfo().BaseType;
            }
        }

        private class InMemoryEventSubscription : EventSubscription
        {
            private readonly Func<object, Task> _handler;
            private readonly Action _unsubscribe;

            public InMemoryEventSubscription(Func<object, Task> handler, Action unsubscribe)
            {
                _handler = handler;
                _unsubscribe = unsubscribe;
            }

            public Task Handle<TEvent>(TEvent evnt)
            {
                return _handler(evnt);
            }

            public void Dispose()
            {
                _unsubscribe();
            }
        }
    }
}