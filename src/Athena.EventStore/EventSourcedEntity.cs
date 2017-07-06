using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Athena.EventStore.ProcessManagers;

namespace Athena.EventStore
{
    public abstract class EventSourcedEntity
    {
        private readonly ICollection<Event> _uncommittedChanges = new Collection<Event>();

        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, Action<object>>> Handlers =
            new ConcurrentDictionary<Type, ConcurrentDictionary<Type, Action<object>>>();

        protected EventSourcedEntity()
        {
            Version = 0;
        }

        public string Id { get; set; }
        public long Version { get; private set; }

        public void BuildFromHistory(IReadOnlyCollection<Event> eventStream)
        {
            if (_uncommittedChanges.Count > 0)
                throw new InvalidOperationException("Cannot apply history when instance has uncommitted changes.");

            foreach (var evnt in eventStream)
                HandleEvent(evnt);
        }

        public IReadOnlyCollection<Event> GetUncommittedChanges()
        {
            return new List<Event>(_uncommittedChanges);
        }

        public void ClearUncommittedChanges()
        {
            _uncommittedChanges.Clear();
        }

        public virtual string GetStreamName()
        {
            return $"{GetType().Name}-{Id}";
        }

        protected void ApplyEvent(object evnt)
        {
            _uncommittedChanges.Add(new Event(Guid.NewGuid(), evnt));
            HandleEvent(evnt);
        }

        private void HandleEvent(object evnt)
        {
            var handler = Handlers
                .GetOrAdd(GetType(), x => new ConcurrentDictionary<Type, Action<object>>())
                .GetOrAdd(evnt.GetType(), x =>
                {
                    var allTypes = x.GetParentTypesFor().ToList();

                    var matchingMethods = GetType()
                        .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(y => y.Name == "On" && y.GetParameters().Length == 1 &&
                                    allTypes.Contains(y.GetParameters()[0].ParameterType))
                        .Select(y => new
                        {
                            Method = y,
                            EventParameter = y.GetParameters()[0]
                        })
                        .ToList();

                    var methods = new List<Action<object>>();

                    foreach (var matchingMethod in matchingMethods)
                    {
                        var parameter = Expression.Parameter(matchingMethod.EventParameter.ParameterType,
                            matchingMethod.EventParameter.Name);

                        var body = Expression.Call(matchingMethod.Method, parameter);
                        
                        methods.Add(Expression.Lambda<Action<object>>(body, parameter).Compile());
                    }

                    return (currentEvent =>
                    {
                        foreach (var method in methods)
                            method(currentEvent);
                    });
                });

            handler(evnt);

            Version++;
        }
    }
}