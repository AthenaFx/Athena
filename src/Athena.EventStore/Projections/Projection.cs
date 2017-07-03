using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Athena.EventStore.Serialization;
using Athena.Logging;

namespace Athena.EventStore.Projections
{
    public abstract class Projection<TState, TIdentity> : EventStoreProjection
    {
        public virtual string Name => GetType().Name.Replace(".", "-").ToLower();

        public abstract IEnumerable<string> GetStreamsToProjectFrom();

        public virtual async Task Apply(DeSerializationResult evnt, IDictionary<string, object> environment)
        {
            Logger.Write(LogLevel.Debug, $"Applying {evnt.Data} to projection {Name}");
            
            var mappings = new Dictionary<Type, Tuple<Func<object, EventContext<TState>, Task>,
                Func<DeSerializationResult, TIdentity>>>();

            MapInterestingEvents(new EventMappingContext<TState, TIdentity>(mappings));

            foreach (var eventType in evnt.Data.GetType().GetParentTypesFor())
            {
                if (!mappings.ContainsKey(eventType))
                    continue;

                var handlerMapping = mappings[eventType];

                var id = handlerMapping.Item2(evnt);

                var projectionInstance = await LoadOrCreate(id, environment).ConfigureAwait(false);

                if (projectionInstance == null)
                    continue;

                await handlerMapping.Item1(evnt,
                        new EventContext<TState>(projectionInstance, evnt.Metadata, evnt.OriginalEvent))
                    .ConfigureAwait(false);
            }
        }

        protected abstract void MapInterestingEvents(EventMappingContext<TState, TIdentity> mappingContext);

        protected abstract Task<TState> LoadOrCreate(TIdentity id, IDictionary<string, object> environment);
    }
}