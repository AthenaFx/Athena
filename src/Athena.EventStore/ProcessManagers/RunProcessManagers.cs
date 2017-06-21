using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.EventStore.Serialization;
using Athena.Logging;
using Athena.Processes;
using EventStore.ClientAPI;

namespace Athena.EventStore.ProcessManagers
{
    public class RunProcessManagers : LongRunningProcess
    {
        private readonly IEnumerable<ProcessManager> _processManagers;
        private readonly EventStoreConnectionString _connectionString;
        private readonly EventSerializer _eventSerializer;
        private readonly AthenaContext _athenaContext;

        private IEventStoreConnection _connection;
        private bool _running;

        private readonly IDictionary<string, ProcessManagerSubscription> _processManagerSubscriptions =
            new Dictionary<string, ProcessManagerSubscription>();

        public RunProcessManagers(IEnumerable<ProcessManager> processManagers,
            EventStoreConnectionString connectionString, EventSerializer eventSerializer, AthenaContext athenaContext)
        {
            _processManagers = processManagers;
            _connectionString = connectionString;
            _eventSerializer = eventSerializer;
            _athenaContext = athenaContext;
        }

        public async Task Start()
        {
            if (_running)
                return;

            _running = true;

            _connection = _connectionString.CreateConnection(x => x
                .KeepReconnecting()
                .KeepRetrying()
                .UseCustomLogger(new EventStoreLog()));

            foreach (var processManager in _processManagers)
                await SetupProcessManagerSubscription(processManager).ConfigureAwait(false);
        }

        public Task Stop()
        {
            _running = false;

            foreach (var subscription in _processManagerSubscriptions)
                subscription.Value.Close();

            _processManagerSubscriptions.Clear();

            _connection.Close();
            _connection.Dispose();
            _connection = null;

            return Task.CompletedTask;
        }

        protected virtual async Task SetupProcessManagerSubscription(ProcessManager processManager)
        {
            while (true)
            {
                if (!_running)
                    return;
                
                if (_processManagerSubscriptions.ContainsKey(processManager.Name))
                {
                    _processManagerSubscriptions[processManager.Name].Close();
                    _processManagerSubscriptions.Remove(processManager.Name);
                }
                
                try
                {
                    var eventStoreSubscription = _connection.ConnectToPersistentSubscription(processManager.Name,
                        processManager.Name,
                        async (subscription, evnt) =>
                            await PushEventToProcessManager(processManager, _eventSerializer.DeSerialize(evnt),
                                subscription).ConfigureAwait(false),
                        async (subscription, reason, exception) =>
                            await SubscriptionDropped(processManager, reason, exception).ConfigureAwait(false),
                        autoAck: false);

                    _processManagerSubscriptions[processManager.Name] =
                        new ProcessManagerSubscription(eventStoreSubscription);

                    return;
                }
                catch (Exception ex)
                {
                    if (!_running)
                        return;

                    Logger.Write(LogLevel.Error,
                        $"Couldn't subscribe processmanager: {processManager.Name}. Retrying in 5 seconds.", ex);

                    await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
            }
        }

        protected virtual async Task PushEventToProcessManager(ProcessManager processManager,
            DeSerializationResult evnt, EventStorePersistentSubscriptionBase subscription)
        {
            if (!_running)
                return;

            if (!evnt.Successful)
            {
                subscription.Fail(evnt.OriginalEvent, PersistentSubscriptionNakEventAction.Unknown, evnt.Error.Message);
                return;
            }

            try
            {
                var requestEnvironment = new Dictionary<string, object>
                {
                    ["context"] = new ProcessManagerExecutionContext(processManager, evnt)
                };

                await _athenaContext.Execute("esprocessmanager", requestEnvironment).ConfigureAwait(false);
                
                subscription.Acknowledge(evnt.OriginalEvent);
            }
            catch (Exception ex)
            {
                Logger.Write(LogLevel.Error, $"Couldn't push event to processmanager: {processManager.Name}", ex);

                subscription.Fail(evnt.OriginalEvent, PersistentSubscriptionNakEventAction.Unknown, ex.Message);
            }
        }

        protected virtual Task SubscriptionDropped(ProcessManager processManager, SubscriptionDropReason reason,
            Exception exception)
        {
            if (!_running)
                return Task.CompletedTask;

            Logger.Write(LogLevel.Warn,
                $"Subscription dropped for processmanager: {processManager.Name}. Reason: {reason}. Retrying...",
                exception);

            if (reason != SubscriptionDropReason.UserInitiated)
                return SetupProcessManagerSubscription(processManager);

            return Task.CompletedTask;
        }
    }
}