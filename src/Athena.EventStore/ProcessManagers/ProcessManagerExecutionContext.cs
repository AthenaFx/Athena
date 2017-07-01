using Athena.EventStore.Serialization;

namespace Athena.EventStore.ProcessManagers
{
    public class ProcessManagerExecutionContext
    {
        public ProcessManagerExecutionContext(ProcessManager processManager, DeSerializationResult evnt, 
            ProcessStateLoader stateLoader)
        {
            ProcessManager = processManager;
            Event = evnt;
            StateLoader = stateLoader;
        }

        public ProcessManager ProcessManager { get; }
        public DeSerializationResult Event { get; }
        public ProcessStateLoader StateLoader { get; }
    }
}