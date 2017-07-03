using System;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.Logging;

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
            Logger.Write(LogLevel.Debug, $"Publishing event {evnt}");
            
            return _publisher.Publish(evnt, context);
        }

        public static EventSubscription Subscribe<TEvent>(Func<TEvent, Task> subscription, string id = null)
        {
            if(string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString();
            
            Logger.Write(LogLevel.Debug, $"Subscribing to event of type {typeof(TEvent)} (id: {id})");
            
            return _publisher.Subscribe<TEvent>((evnt, context) => subscription(evnt), id);
        }
        
        public static EventSubscription Subscribe<TEvent>(Func<TEvent, SettingsContext, Task> subscription, 
            string id = null)
        {
            if(string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString();
            
            return _publisher.Subscribe(subscription, id);
        }

        public static void UnSubscribe(string id)
        {
            Logger.Write(LogLevel.Debug, $"Unsubscribing {id}");
            
            _publisher.UnSubscribe(id);
        }
    }
}