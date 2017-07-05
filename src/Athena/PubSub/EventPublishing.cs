using System;
using System.Collections.Concurrent;
using Athena.Logging;

namespace Athena.PubSub
{
    public static class EventPublishing
    {
        private static readonly ConcurrentDictionary<Type, EventObserver> EventObservers =
            new ConcurrentDictionary<Type, EventObserver>();

        public static void Publish(object evnt)
        {
            Logger.Write(LogLevel.Debug, $"Publishing event {evnt}");

            var types = evnt.GetType().GetParentTypesFor();

            foreach (var type in types)
            {
                EventObserver observer;
                if (EventObservers.TryGetValue(type, out observer))
                    observer.OnNext(evnt);
            }
        }

        public static IObservable<TEvent> OpenChannel<TEvent>()
        {
            Logger.Write(LogLevel.Debug, $"Subscribing to event of type {typeof(TEvent)}");
            
            var eventObserver = EventObservers.GetOrAdd(typeof(TEvent), x => new EventObserver());
            
            return new EventObservable<TEvent>(eventObserver);
        }
        
        private class EventObservable<TEvent> : IObservable<TEvent>, IDisposable
        {
            private readonly EventObserver _eventObserver;
            private readonly string _id = Guid.NewGuid().ToString();

            public EventObservable(EventObserver eventObserver)
            {
                _eventObserver = eventObserver;
            }

            public IDisposable Subscribe(IObserver<TEvent> observer)
            {
                _eventObserver.AddSubscriber(observer, _id);

                return this;
            }

            public void Dispose()
            {
                _eventObserver.RemoveSubscriber(_id);
            }
        }

        private class EventObserver
        {
            private readonly ConcurrentDictionary<string, IObserver<object>> _observers =
                new ConcurrentDictionary<string, IObserver<object>>();

            public void AddSubscriber<TEvent>(IObserver<TEvent> observable, string id)
            {
                _observers[id] = new IntermidiatObserver<TEvent>(observable);
            }

            public void RemoveSubscriber(string id)
            {
                IObserver<object> observer;
                _observers.TryRemove(id, out observer);
            }

            public void OnNext(object value)
            {
                foreach (var observer in _observers)
                    observer.Value.OnNext(value);
            }
        
            private class IntermidiatObserver<TEvent> : IObserver<object>
            {
                private readonly IObserver<TEvent> _inner;

                public IntermidiatObserver(IObserver<TEvent> inner)
                {
                    _inner = inner;
                }

                public void OnCompleted()
                {
                    _inner.OnCompleted();
                }

                public void OnError(Exception error)
                {
                    _inner.OnError(error);
                }

                public void OnNext(object value)
                {
                    _inner.OnNext((TEvent) value);
                }
            }
        }

    }
}