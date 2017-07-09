using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Logging;

namespace Athena.PubSub
{
    public static class EventPublishing
    {
        private static readonly ConcurrentDictionary<Type, EventObserver> EventObservers =
            new ConcurrentDictionary<Type, EventObserver>();

        public static void Publish<TEvent>(TEvent evnt, IDictionary<string, object> environment)
        {
            Logger.Write(LogLevel.Debug, $"Publishing event {evnt}");

            var types = evnt.GetType().GetParentTypesFor();

            foreach (var type in types)
            {
                EventObserver observer;
                if (EventObservers.TryGetValue(type, out observer))
                    observer.OnNext(evnt, environment);
            }
        }

        public static IObservable<EventData<TEvent>> OpenChannel<TEvent>()
        {
            Logger.Write(LogLevel.Debug, $"Subscribing to event of type {typeof(TEvent)}");

            var eventObserver = EventObservers.GetOrAdd(typeof(TEvent), x => new EventObserver());

            return new EventObservable<TEvent>(eventObserver);
        }

        private class EventObservable<TEvent> : IObservable<EventData<TEvent>>, IDisposable
        {
            private readonly EventObserver _eventObserver;
            private readonly string _id = Guid.NewGuid().ToString();

            public EventObservable(EventObserver eventObserver)
            {
                _eventObserver = eventObserver;
            }

            public IDisposable Subscribe(IObserver<EventData<TEvent>> observer)
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
            private readonly ConcurrentDictionary<string, IObserver<EventData<object>>> _observers =
                new ConcurrentDictionary<string, IObserver<EventData<object>>>();

            public void AddSubscriber<TEvent>(IObserver<EventData<TEvent>> observable, string id)
            {
                _observers[id] = new IntermidiatObserver<TEvent>(observable);
            }

            public void RemoveSubscriber(string id)
            {
                IObserver<EventData<object>> observer;
                if(!_observers.TryRemove(id, out observer))
                    return;
                
                observer.OnCompleted();
            }

            public void OnNext(object value, IDictionary<string, object> environment)
            {
                foreach (var observer in _observers)
                {
                    try
                    {
                        observer.Value.OnNext(new EventData<object>(value, environment));
                    }
                    catch (Exception e)
                    {
                        Logger.Write(LogLevel.Warn, $"Failed to handle event of type {value.GetType()}", e);
                        
                        observer.Value.OnError(e);
                    }   
                }
            }

            private class IntermidiatObserver<TEvent> : IObserver<EventData<object>>
            {
                private readonly IObserver<EventData<TEvent>> _inner;

                public IntermidiatObserver(IObserver<EventData<TEvent>> inner)
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

                public void OnNext(EventData<object> value)
                {
                    _inner.OnNext(new EventData<TEvent>((TEvent)value.Event, value.Environment));
                }
            }
        }
    }
}