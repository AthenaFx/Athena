using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Athena.EventStore.ProcessManagers
{
    public class ProcessManagerEventMappingContext<TState, TIdentity>
    {
        private readonly
            IDictionary<Type, Tuple<Func<object, EventProcessingContext<TState>, Task>, Func<object, TIdentity>, string>>
            _eventHandlerMappings;

        public ProcessManagerEventMappingContext(IDictionary<Type, Tuple<Func<object, EventProcessingContext<TState>, Task>, 
            Func<object, TIdentity>, string>> eventHandlerMappings)
        {
            _eventHandlerMappings = eventHandlerMappings;
        }

        public void MapEventTo<TEvent>(Func<TEvent, EventProcessingContext<TState>, Task> onArrived, 
            Expression<Func<TEvent, TIdentity>> findId) where TEvent : class
        {
            var member = "";

            var memberExpression = findId.Body as MemberExpression;

            if (memberExpression != null)
                member = memberExpression.Member.Name;

            var findIdFunc = findId.Compile();

            _eventHandlerMappings[typeof(TEvent)] =
                new Tuple<Func<object, EventProcessingContext<TState>, Task>, Func<object, TIdentity>, string>(
                    (x, y) => onArrived((TEvent) x, y), x => findIdFunc((TEvent) x), member);
        }
    }
}