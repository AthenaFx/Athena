using System;

namespace Athena.Timeouts
{
    public class TimeoutData
    {
        public TimeoutData(Guid id, object message, DateTime at)
        {
            Id = id;
            Message = message;
            At = at;
        }

        public Guid Id { get; }
        public object Message { get; }
        public DateTime At { get; }
    }
}