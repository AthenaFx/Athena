using System;
using System.Threading.Tasks;

namespace Athena.PubSub
{
    public class EventSubscription : IDisposable
    {
        private readonly Func<object, SettingsContext, Task> _handle;
        private readonly Action<string> _unsubscribe;
        private bool _isDisposed;
        
        public EventSubscription(Func<object, SettingsContext, Task> handle, Action<string> unsubscribe, string id, 
            Type subscribedTo)
        {
            _handle = handle;
            _unsubscribe = unsubscribe;
            Id = id;
            SubscribedTo = subscribedTo;
        }

        internal Task Handle(object evnt, SettingsContext context)
        {
            return _handle(evnt, context);
        }

        public string Id { get; }
        internal Type SubscribedTo { get; }

        public void Dispose()
        {
            if (_isDisposed) return;
            
            _unsubscribe(Id);

            _isDisposed = true;
        }
    }
}