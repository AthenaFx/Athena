using System;

namespace Athena.EventStore.ProcessManagers
{
    public class Event
    {
        public Event(Guid id, object instance)
        {
            Id = id;
            Instance = instance;
        }

        public Guid Id { get; }
        public object Instance { get; }
    }
}