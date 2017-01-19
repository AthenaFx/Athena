using System;
using System.Threading.Tasks;

namespace Athena
{
    public interface EventBus
    {
        Task Publish<TEvent>(TEvent evnt);
        EventSubscription Subscribe<TEvent>(Func<TEvent, Task> subscriber);
    }
}