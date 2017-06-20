using System;
using Athena.EventStore.Serialization;

namespace Athena.EventStore.Projections
{
    public class MessageProcessor
    {
        public event Action<DeSerializationResult> MessageArrived;
        public event Action<long> MessageHandled;

        public virtual void OnMessageArrived(DeSerializationResult obj)
        {
            MessageArrived?.Invoke(obj);
        }

        public virtual void OnMessageHandled(long obj)
        {
            MessageHandled?.Invoke(obj);
        }
    }
}