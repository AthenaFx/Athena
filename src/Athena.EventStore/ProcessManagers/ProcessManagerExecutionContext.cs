using Athena.EventStore.Serialization;

namespace Athena.EventStore.ProcessManagers
{
    public class ProcessManagerExecutionContext
    {
        public ProcessManagerExecutionContext(ProcessManager processManager, DeSerializationResult evnt)
        {
            ProcessManager = processManager;
            Event = evnt;
        }

        public ProcessManager ProcessManager { get; }
        public DeSerializationResult Event { get; }
    }
}