using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Athena.ApplicationTimeouts;
using Athena.EventStore.Serialization;
using EventStore.ClientAPI;

namespace Athena.EventStore.ProcessManagers
{
    public abstract class EventStoreProcessManager<TState> : ProcessManager 
        where TState : EventSourcedEntity, new()
    {
        public virtual string Name => GetType().FullName.Replace(".", "-").ToLower();
        
        public virtual async Task Handle(DeSerializationResult evnt, IDictionary<string, object> environment, 
            AthenaContext context, EventSerializer serializer, IEventStoreConnection connection)
        {
            var timeoutManager = context.GetSetting<TimeoutManager>();
            
            var eventMappings = new Dictionary<Type, Tuple<Func<object, EventProcessingContext<TState>, Task>, 
                Func<object, string>, string>>();
            
            var mappingContext = new ProcessManagerEventMappingContext<TState, string>(eventMappings);

            MapEvents(mappingContext);

            foreach (var type in evnt.Data.GetType().GetParentTypesFor())
            {
                if (!eventMappings.ContainsKey(type))
                    continue;

                var handlerMapping = eventMappings[type];

                var id = handlerMapping.Item2(evnt);

                var state = await connection.Load<TState>(id, serializer).ConfigureAwait(false);
                
                if(state == null)
                    continue;

                await handlerMapping
                    .Item1(evnt, new EventProcessingContext<TState>(evnt, environment, state, 
                        timeoutManager.RequestTimeout))
                    .ConfigureAwait(false);
            }
        }
        
        public abstract IEnumerable<string> GetInterestingStreams();
        
        public IReadOnlyDictionary<Type, string> GetEventMappings()
        {
            var eventMappings = new Dictionary<Type, Tuple<Func<object, EventProcessingContext<TState>, Task>, 
                Func<object, string>, string>>();
            
            var mappingContext = new ProcessManagerEventMappingContext<TState, string>(eventMappings);

            MapEvents(mappingContext);

            return new ReadOnlyDictionary<Type, string>(eventMappings.ToDictionary(x => x.Key, x => x.Value.Item3));
        }

        protected abstract void MapEvents(ProcessManagerEventMappingContext<TState, string> mappingContext);
    }
}