using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Configuration;
using Athena.EventStore.Serialization;
using Athena.MetaData;
using Athena.Transactions;

namespace Athena.EventStore.ProcessManagers
{
    public class ProcessManagersSettings : AppFunctionDefinition
    {
        private readonly ICollection<ProcessManager> _processManagers = new List<ProcessManager>();
        private EventSerializer _serializer = new JsonEventSerializer();
        private EventStoreConnectionString _connectionString 
            = new EventStoreConnectionString("Ip=127.0.0.1;Port=1113;UserName=admin;Password=changeit;");

        private Func<ProcessManagersSettings, ProcessStateLoader> _getStateLoader = (settings) =>
            new LoadProcessManagerStateFromEventStore(settings.GetConnectionString().CreateConnection());

        public ProcessManagersSettings LoadStateWith(
            Func<ProcessManagersSettings, ProcessStateLoader> getStateLoader)
        {
            _getStateLoader = getStateLoader;

            return this;
        }

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
        
        internal ProcessStateLoader GetStateLoader()
        {
            return _getStateLoader(this);
        }

        public override string Name { get; } = "esprocessmanager";
        
        protected override AppFunctionBuilder DefineDefaultApplication(AppFunctionBuilder builder)
        {
            return builder
                .Last("HandleTransactions", next => new HandleTransactions(next,
                    Enumerable.Empty<Transaction>()).Invoke)
                .Last("SupplyMetaData", next => new SupplyMetaData(next).Invoke)
                .Last("ExecuteProcessManager", next => new ExecuteProcessManager(next).Invoke);
        }
    }
}