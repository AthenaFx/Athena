using Athena.EventStore.Serialization;
using EventStore.ClientAPI;

namespace Athena.EventStore.ProcessManagers
{
    public class ProcessManagerExecutionContext
    {
        public ProcessManagerExecutionContext(ProcessManager processManager, DeSerializationResult evnt, 
            IEventStoreConnection connection, EventSerializer serializer)
        {
            ProcessManager = processManager;
            Event = evnt;
            Connection = connection;
            Serializer = serializer;
        }

        public ProcessManager ProcessManager { get; }
        public DeSerializationResult Event { get; }
        public IEventStoreConnection Connection { get; }
        public EventSerializer Serializer { get; }
    }
}