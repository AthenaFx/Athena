namespace Athena.EventStore.ProcessManagers
{
    public class ProcessManagerTimedOut
    {
        public ProcessManagerTimedOut(string process, object @event)
        {
            Process = process;
            Event = @event;
        }

        public string Process { get; }
        public object Event { get; }
    }
}