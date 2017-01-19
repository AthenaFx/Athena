using System;
using System.Threading.Tasks;

namespace Athena
{
    public static class Events
    {
        private static EventBus _eventBus = new InMemoryEventBus();

        public static void ConfigureEventBus(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public static Task Publish<TEvent>(TEvent evnt)
        {
            return _eventBus.Publish(evnt);
        }

        public static EventSubscription Subscribe<TEvent>(Func<TEvent, Task> subscriber)
        {
            return _eventBus.Subscribe(subscriber);
        }
    }
}