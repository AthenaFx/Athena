using System;
using System.Threading.Tasks;

namespace Athena.PubSub
{
    public interface EventPublisher
    {
        Task Publish(object evnt);
        EventSubscription Subscribe<TEvent>(Func<TEvent, Task> subscription, string id = null);
        void UnSubscribe(string id);
    }
}