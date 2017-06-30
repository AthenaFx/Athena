using System.Collections.Generic;
using System.Linq;
using Athena.EventStore.Serialization;

namespace Athena.EventStore.ProcessManagers
{
    public class ProcessManagersSettings
    {
        private readonly ICollection<ProcessManager> _processManagers = new List<ProcessManager>();
        private EventSerializer _serializer = new JsonEventSerializer();
        private EventStoreConnectionString _connectionString 
            = new EventStoreConnectionString("Ip=127.0.0.1;Port=1113;UserName=admin;Password=changeit;");

        public ProcessManagersSettings WithSerializer(EventSerializer serializer)
        {
            _serializer = serializer;

            return this;
        }

        public ProcessManagersSettings WithConnectionString(string connectionString)
        {
            _connectionString = new EventStoreConnectionString(connectionString);

            return this;
        }

        public ProcessManagersSettings WithProcessManager(ProcessManager processManager)
        {
            _processManagers.Add(processManager);

            return this;
        }

        public EventSerializer GetSerializer()
        {
            return _serializer;
        }

        public EventStoreConnectionString GetConnectionString()
        {
            return _connectionString;
        }

        public IReadOnlyCollection<ProcessManager> GetProcessManagers()
        {
            return _processManagers.ToList();
        }
    }
}