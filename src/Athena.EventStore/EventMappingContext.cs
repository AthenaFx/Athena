using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.EventStore.Serialization;

namespace Athena.EventStore
{
    public class EventMappingContext<TState, TIdentity>
    {
        private readonly
            IDictionary<Type, Tuple<Func<object, EventContext<TState>, Task>,
                Func<DeSerializationResult, TIdentity>>> _eventHandlerMappings;

        public EventMappingContext(
            IDictionary<Type, Tuple<Func<object, EventContext<TState>, Task>,
                Func<DeSerializationResult, TIdentity>>> eventHandlerMappings)
        {
            _eventHandlerMappings = eventHandlerMappings;
        }

        public void MapEventTo<TEvent>(Func<TEvent, EventContext<TState>, Task> onArrived,
            Func<TEvent, TIdentity> findId) where TEvent : class
        {
            _eventHandlerMappings[typeof(TEvent)] =
                new Tuple<Func<object, EventContext<TState>, Task>, Func<DeSerializationResult, TIdentity>>(
                    (x, y) => onArrived((TEvent) x, y), x => findId((TEvent) x.Data));
        }

        public void MapEventTo<TEvent>(Func<TEvent, EventContext<TState>, Task> onArrived,
            Func<TEvent, DeSerializationResult, TIdentity> findId) where TEvent : class
        {
            _eventHandlerMappings[typeof(TEvent)] =
                new Tuple<Func<object, EventContext<TState>, Task>, Func<DeSerializationResult, TIdentity>>(
                    (x, y) => onArrived((TEvent) x, y), x => findId((TEvent) x.Data, x));
        }
    }
}