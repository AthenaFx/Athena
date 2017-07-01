using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Athena.ApplicationTimeouts;
using Athena.EventStore.Serialization;

namespace Athena.EventStore.ProcessManagers
{
    public abstract class EventStoreProcessManager<TState, TIdentity> : ProcessManager where TState : new()
    {
        public virtual string Name => GetType().FullName.Replace(".", "-").ToLower();
        
        public virtual async Task Handle(DeSerializationResult evnt, IDictionary<string, object> environment,
            ProcessStateLoader stateLoader)
        {
            var eventMappings = new Dictionary<Type, Tuple<Func<object, EventProcessingContext<TState>, Task>, 
                Func<object, TIdentity>, string>>();
            
            var mappingContext = new ProcessManagerEventMappingContext<TState, TIdentity>(eventMappings);

            MapEvents(mappingContext);

            foreach (var type in evnt.Data.GetType().GetParentTypesFor())
            {
                if (!eventMappings.ContainsKey(type))
                    continue;

                var handlerMapping = eventMappings[type];

                var id = handlerMapping.Item2(evnt);

                var state = await stateLoader.Load<TState, TIdentity>(id).ConfigureAwait(false);
                
                if(state == null)
                    continue;

                await handlerMapping
                    .Item1(evnt, new EventProcessingContext<TState>(evnt, environment, state))
                    .ConfigureAwait(false);
            }
        }
        
        public abstract IEnumerable<string> GetInterestingStreams();
        
        public IReadOnlyDictionary<Type, string> GetEventMappings()
        {
            var eventMappings = new Dictionary<Type, Tuple<Func<object, EventProcessingContext<TState>, Task>, 
                Func<object, TIdentity>, string>>();
            
            var mappingContext = new ProcessManagerEventMappingContext<TState, TIdentity>(eventMappings);

            MapEvents(mappingContext);

            return new ReadOnlyDictionary<Type, string>(eventMappings.ToDictionary(x => x.Key, x => x.Value.Item3));
        }

        protected abstract void MapEvents(ProcessManagerEventMappingContext<TState, TIdentity> mappingContext);

        protected virtual Task RequestTimeout(object evnt, DateTime at)
        {
            return Timeouts.RequestTimeout(new ProcessManagerTimedOut(Name, evnt), at);
        }

        protected virtual Task RequestTimeout(object evnt, TimeSpan @in)
        {
            return RequestTimeout(evnt, DateTime.UtcNow + @in);
        }
    }
}