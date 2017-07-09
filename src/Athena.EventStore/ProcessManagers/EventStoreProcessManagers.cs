using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.EventStore.Serialization;
using Athena.Logging;
using Athena.MetaData;
using Athena.Transactions;
using EventStore.ClientAPI;

namespace Athena.EventStore.ProcessManagers
{
    public class EventStoreProcessManagers : AppFunctionDefinition
    {
        private IEventStoreConnection _connection;
        private bool _running;

        private readonly IDictionary<string, ProcessManagerSubscription> _processManagerSubscriptions =
            new Dictionary<string, ProcessManagerSubscription>();
        
        private readonly ICollection<Transaction> _transactions = new List<Transaction>();
        private readonly ICollection<Tuple<ProcessManager, int>> _processManagers 
            = new List<Tuple<ProcessManager, int>>();
        private readonly ICollection<MetaDataSupplier> _metaDataSuppliers = new List<MetaDataSupplier>();
        private EventSerializer _serializer = new JsonEventSerializer();
        private EventStoreConnectionString _connectionString 
            = new EventStoreConnectionString("Ip=127.0.0.1;Port=1113;UserName=admin;Password=changeit;");

        public EventStoreProcessManagers HandleTransactionsWith(Transaction transaction)
        {
            _transactions.Add(transaction);

            return this;
        }

        public EventStoreProcessManagers SupplyMetaDataWith(MetaDataSupplier supplier)
        {
            _metaDataSuppliers.Add(supplier);

            return this;
        }

        public EventStoreProcessManagers WithSerializer(EventSerializer serializer)
        {
            _serializer = serializer;

            return this;
        }

        public EventStoreProcessManagers WithConnectionString(string connectionString)
        {
            _connectionString = new EventStoreConnectionString(connectionString);

            return this;
        }

        public EventStoreProcessManagers WithProcessManager(ProcessManager processManager, int workers = 1)
        {
            _processManagers.Add(new Tuple<ProcessManager, int>(processManager, workers));

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

        public string Name { get; } = "esprocessmanager";
        
        protected override AppFunctionBuilder DefineDefaultApplication(AppFunctionBuilder builder)
        {
            return builder
                .First("HandleTransactions", next => new HandleTransactions(next,
                    _transactions.ToList()).Invoke, () => _transactions.GetDiagnosticsData())
                .ContinueWith("SupplyMetaData", next => new SupplyMetaData(next, _metaDataSuppliers.ToList()).Invoke,
                    () => _metaDataSuppliers.GetDiagnosticsData())
                .Last("ExecuteProcessManager", next => new ExecuteProcessManager(next).Invoke);
        }

        public async Task Start(AthenaContext context)
        {
            if (_running)
                return;
            
            Logger.Write(LogLevel.Debug, "Starting process managers");

            _running = true;

            _connection = GetConnectionString().CreateConnection(x => x
                .KeepReconnecting()
                .KeepRetrying()
                .UseCustomLogger(new EventStoreLog()));

            foreach (var processManager in _processManagers)
            {
                foreach (var worker in Enumerable.Range(1, processManager.Item2))
                {
                    await SetupProcessManagerSubscription(processManager.Item1, context, $"worker-{worker}")
                        .ConfigureAwait(false);
                }
            }
        }

        public Task Stop()
        {
            Logger.Write(LogLevel.Debug, "Stopping process managers");
            
            _running = false;

            foreach (var subscription in _processManagerSubscriptions)
                subscription.Value.Close();

            _processManagerSubscriptions.Clear();

            _connection.Close();
            _connection.Dispose();
            _connection = null;

            return Task.CompletedTask;
        }

        protected virtual async Task SetupProcessManagerSubscription(ProcessManager processManager, 
            AthenaContext context, string worker)
        {
            var key = $"{processManager.Name}-{worker}";
            
            while (true)
            {
                if (!_running)
                    return;
                
                Logger.Write(LogLevel.Debug, $"Starting process manager {processManager.Name}, worker {worker}");
                
                if (_processManagerSubscriptions.ContainsKey(key))
                {
                    _processManagerSubscriptions[key].Close();
                    _processManagerSubscriptions.Remove(key);
                }
                
                try
                {
                    var eventStoreSubscription =  await _connection.ConnectToPersistentSubscriptionAsync(processManager.Name,
                        processManager.Name,
                        (subscription, evnt) =>
                            PushEventToProcessManager(processManager, GetSerializer().DeSerialize(evnt),
                                subscription, context).GetAwaiter().GetResult(),
                        (subscription, reason, exception) =>
                            SubscriptionDropped(processManager, reason, exception, context, worker)
                                .GetAwaiter().GetResult(),
                        autoAck: false).ConfigureAwait(false);

                    _processManagerSubscriptions[key] =
                        new ProcessManagerSubscription(eventStoreSubscription);

                    return;
                }
                catch (Exception ex)
                {
                    if (!_running)
                        return;

                    Logger.Write(LogLevel.Warn,
                        $"Couldn't subscribe processmanager: {processManager.Name}. Retrying in 5 seconds.", ex);

                    await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
            }
        }

        protected virtual async Task PushEventToProcessManager(ProcessManager processManager,
            DeSerializationResult evnt, EventStorePersistentSubscriptionBase subscription,
            AthenaContext context)
        {
            if (!_running)
                return;

            if (!evnt.Successful)
            {
                Logger.Write(LogLevel.Info, "Event deserialization failed", evnt.Error);
                
                subscription.Fail(evnt.OriginalEvent, PersistentSubscriptionNakEventAction.Unknown, evnt.Error.Message);
                return;
            }

            try
            {
                var requestEnvironment = new Dictionary<string, object>
                {
                    ["context"] = new ProcessManagerExecutionContext(processManager, evnt, _connection, _serializer)
                };

                Logger.Write(LogLevel.Debug,
                    $"Execution process manager application {Name} for {processManager.Name}");
                
                await context.Execute(Name, requestEnvironment).ConfigureAwait(false);

                Logger.Write(LogLevel.Debug,
                    $"Process manager, \"{processManager.Name}\", executed. Application {Name}");
                
                subscription.Acknowledge(evnt.OriginalEvent);
            }
            catch (Exception ex)
            {
                Logger.Write(LogLevel.Error, $"Couldn't push event to processmanager: {processManager.Name}", ex);

                subscription.Fail(evnt.OriginalEvent, PersistentSubscriptionNakEventAction.Unknown, ex.Message);
            }
        }

        protected virtual Task SubscriptionDropped(ProcessManager processManager, SubscriptionDropReason reason,
            Exception exception, AthenaContext context, string worker)
        {
            if (!_running)
                return Task.CompletedTask;

            Logger.Write(LogLevel.Warn,
                $"Subscription dropped for processmanager: {processManager.Name}. Reason: {reason}. Retrying...",
                exception);

            if (reason != SubscriptionDropReason.UserInitiated)
                return SetupProcessManagerSubscription(processManager, context, worker);

            return Task.CompletedTask;
        }
    }
}