using System;
using System.Threading.Tasks;

namespace Athena.PubSub
{
    public interface EventPublisher
    {
        Task Publish<TEvent>(TEvent evnt);
        EventSubscription Subscribe<TEvent>(Func<TEvent, Task> subscription, string id = null);
        void UnSubscribe(string id);
    }
}