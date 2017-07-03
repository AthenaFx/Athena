using System;
using System.Threading.Tasks;

namespace Athena.PubSub
{
    public interface EventPublisher
    {
        Task Publish(object evnt, SettingsContext context);
        EventSubscription Subscribe<TEvent>(Func<TEvent, SettingsContext, Task> subscription, string id);
        void UnSubscribe(string id);
    }
}