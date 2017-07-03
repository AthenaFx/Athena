using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Configuration;
using Athena.EventStore.Serialization;
using Athena.Logging;
using Athena.MetaData;
using Athena.Resources;
using Athena.Routing;
using Athena.Transactions;
using EventStore.ClientAPI;

namespace Athena.EventStore.StreamSubscriptions
{
    public class RunStreamLiveSubscribers : AppFunctionDefinition
    {
        private readonly IDictionary<string, IServiceSubscription> _serviceSubscriptions =
            new Dictionary<string, IServiceSubscription>();

        private bool _running;
        private IEventStoreConnection _connection;
        
        private readonly ICollection<Transaction> _transactions = new List<Transaction>();
        private readonly ICollection<Tuple<string, int>> _streams = new List<Tuple<string, int>>();
        private readonly ICollection<MetaDataSupplier> _metaDataSuppliers = new List<MetaDataSupplier>();
        private EventSerializer _serializer = new JsonEventSerializer();
        private EventStoreConnectionString _connectionString 
            = new EventStoreConnectionString("Ip=127.0.0.1;Port=1113;UserName=admin;Password=changeit;");
        
        public RunStreamLiveSubscribers HandleTransactionsWith(Transaction transaction)
        {
            _transactions.Add(transaction);

            return this;
        }

        public RunStreamLiveSubscribers SupplyMetaDataWith(MetaDataSupplier supplier)
        {
            _metaDataSuppliers.Add(supplier);

            return this;
        }
        
        public RunStreamLiveSubscribers SubscribeToStream(string stream, int workers = 1)
        {
            _streams.Add(new Tuple<string, int>(stream, workers));

            return this;
        }

        public RunStreamLiveSubscribers WithSerializer(EventSerializer serializer)
        {
            _serializer = serializer;

            return this;
        }

        public RunStreamLiveSubscribers WithConnectionString(string connectionString)
        {
            _connectionString = new EventStoreConnectionString(connectionString);

            return this;
        }

        public IReadOnlyCollection<Tuple<string, int>> GetSubscribedStreams()
        {
            return new ReadOnlyCollection<Tuple<string, int>>(_streams.ToList());
        }

        public EventSerializer GetSerializer()
        {
            return _serializer;
        }

        public EventStoreConnectionString GetConnectionString()
        {
            return _connectionString;
        }

        public string Name { get; } = "eslivesubscription";
        
        protected override AppFunctionBuilder DefineDefaultApplication(AppFunctionBuilder builder)
        {
            var routers = new List<EnvironmentRouter>
            {
                RouteEventToMethod.New(x => x.Name == "Subscribe"
                                            && (x.ReturnType == typeof(void) || x.ReturnType == typeof(Task))
                                            && x.GetParameters().Length == 1, builder.Bootstrapper.ApplicationAssemblies)
            };

            var binders = new List<EnvironmentDataBinder>
            {
                new BindEnvironment(),
                new EventDataBinder()
            };

            var resourceExecutors = new List<ResourceExecutor>
            {
                new MethodResourceExecutor(binders)
            };

            //TODO:Make sure we can have multiple subscribers to same event
            return builder
                .First("Retry", next => new Retry(next, 5, TimeSpan.FromSeconds(1), "Subscription failed").Invoke)
                .ContinueWith("HandleTransactions",
                    next => new HandleTransactions(next, _transactions.ToList()).Invoke,
                    () => _transactions.GetDiagnosticsData())
                .ContinueWith("SupplyMetaData", next => new SupplyMetaData(next, _metaDataSuppliers.ToList()).Invoke,
                    () => _metaDataSuppliers.GetDiagnosticsData())
                .ContinueWith("RouteToResource", next => new RouteToResource(next, routers).Invoke)
                .Last("ExecuteResource", next => new ExecuteResource(next, resourceExecutors).Invoke,
                    () => resourceExecutors.GetDiagnosticsData());
        }

        public async Task Start(AthenaContext context)
        {
            if (_running)
                return;
            
            Logger.Write(LogLevel.Debug, "Starting EventStore live subscriptions");

            _running = true;

            _connection = GetConnectionString().CreateConnection(x => x
                .KeepReconnecting()
                .KeepRetrying()
                .UseCustomLogger(new EventStoreLog()));

            var streams = GetSubscribedStreams();

            foreach (var stream in streams)
            {
                await SetupSubscription(stream.Item1, stream.Item2, context).ConfigureAwait(false);
            }
        }

        public Task Stop()
        {
            Logger.Write(LogLevel.Debug, "Stopping EventStore live subscriptions");
            
            _running = false;

            foreach (var subscription in _serviceSubscriptions)
                subscription.Value.Close();

            _serviceSubscriptions.Clear();

            _connection.Close();
            _connection.Dispose();
            _connection = null;

            return Task.CompletedTask;
        }

        private async Task SetupSubscription(string stream, int workers, AthenaContext context)
        {
            //TODO:Handle multiple workers
            while (true)
            {
                if (!_running)
                    return;
                
                Logger.Write(LogLevel.Debug, $"Subscribing to stream {stream}");

                try
                {
                    if (_serviceSubscriptions.ContainsKey(stream))
                    {
                        _serviceSubscriptions[stream].Close();
                        _serviceSubscriptions.Remove(stream);
                    }

                    //TODO:Handle failing messages
                    var eventstoreSubscription = await _connection.SubscribeToStreamAsync(stream, true,
                            async (subscription, evnt)
                                => await HandleEvent(GetSerializer().DeSerialize(evnt),
                                        x => Logger.Write(LogLevel.Debug,
                                            $"Successfully handled event: {x.OriginalEvent.Event.EventId} on stream: {stream}"),
                                        (x, exception)
                                            => Logger.Write(LogLevel.Error,
                                                $"Failed handling event: {x.OriginalEvent.Event.EventId} on stream: {stream}",
                                                exception), context)
                                    .ConfigureAwait(false),
                            async (subscription, reason, exception)
                                => await SubscriptionDropped(stream, workers, reason, exception, context)
                                    .ConfigureAwait(false))
                        .ConfigureAwait(false);

                    _serviceSubscriptions[stream] = new LiveOnlyServiceSubscription(eventstoreSubscription);
                }
                catch (Exception ex)
                {
                    if (!_running)
                        return;

                    Logger.Write(LogLevel.Warn, $"Couldn't subscribe to stream: {stream}. Retrying in 5 seconds.", ex);

                    await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
            }
        }

        private async Task SubscriptionDropped(string stream, int workers, SubscriptionDropReason reason,
            Exception exception, AthenaContext context)
        {
            if (!_running)
                return;

            Logger.Write(LogLevel.Warn, $"Subscription dropped for stream: {stream}. Reason: {reason}. Retrying...",
                exception);

            if (reason != SubscriptionDropReason.UserInitiated)
                await SetupSubscription(stream, workers, context).ConfigureAwait(false);
        }

        private async Task HandleEvent(DeSerializationResult evnt, Action<DeSerializationResult> done, 
            Action<DeSerializationResult, Exception> error, AthenaContext context)
        {
            if (!_running)
                return;

            if (!evnt.Successful)
            {
                error(evnt, evnt.Error);
                return;
            }

            try
            {
                var requestEnvironment = new Dictionary<string, object>
                {
                    ["event"] = evnt
                };
                
                Logger.Write(LogLevel.Debug,
                    $"Executing live subscription application {Name} for event {evnt.Data}");

                await context.Execute(Name, requestEnvironment).ConfigureAwait(false);

                done(evnt);
                
                Logger.Write(LogLevel.Debug, $"Live subscription application executed for event {evnt.Data}");
            }
            catch (Exception ex)
            {
                Logger.Write(LogLevel.Error, $"Couldn't handle event: {evnt.Data.GetType().FullName}", ex);
                error(evnt, ex);
            }
        }
    }
}