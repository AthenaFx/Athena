using System;
using System.Threading.Tasks;
using Athena.Configuration;

namespace Athena.PubSub
{
    public static class EventPublishing
    {
        private static EventPublisher _publisher = new InMemoryEventPublisher();

        public static AthenaBootstrapper UseEventPublisher(this AthenaBootstrapper bootstrapper, 
            EventPublisher publisher)
        {
            _publisher = publisher;

            return bootstrapper;
        }

        public static Task Publish(this SettingsContext context, object evnt)
        {
            return _publisher.Publish(evnt, context);
        }

        public static EventSubscription Subscribe<TEvent>(Func<TEvent, Task> subscription, string id = null)
        {
            return _publisher.Subscribe<TEvent>((evnt, context) => subscription(evnt), id);
        }
        
        public static EventSubscription Subscribe<TEvent>(Func<TEvent, SettingsContext, Task> subscription, 
            string id = null)
        {
            return _publisher.Subscribe(subscription, id);
        }

        public static void UnSubscribe(string id)
        {
            _publisher.UnSubscribe(id);
        }
    }
}