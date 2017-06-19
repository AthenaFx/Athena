using System;
using System.Threading.Tasks;

namespace Athena.PubSub
{
    public static class EventPublishing
    {
        private static EventPublisher _publisher = new InMemoryEventPublisher();

        public static AthenaBootstrapper UseEventPublisher(this AthenaBootstrapper bootstrapper, EventPublisher publisher)
        {
            _publisher = publisher;

            return bootstrapper;
        }

        public static Task Publish<TEvent>(TEvent evnt)
        {
            return _publisher.Publish(evnt);
        }

        public static Task<EventSubscription> Subscribe<TEvent>(Func<TEvent, Task> subscription, string id = null)
        {
            return _publisher.Subscribe(subscription, id);
        }

        public static void UnSubscribe(string id)
        {
            _publisher.UnSubscribe(id);
        }
    }
}