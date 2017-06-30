namespace Athena.EventStore.ProcessManagers
{
    public class ProcessManagerTimedOut
    {
        public ProcessManagerTimedOut(string process, object evnt)
        {
            Process = process;
            Event = evnt;
        }

        public string Process { get; }
        public object Event { get; }
    }
}