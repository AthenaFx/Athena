using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.EventStore.Serialization;
using Athena.Logging;
using Athena.MetaData;
using Athena.PubSub;
using Athena.Transactions;
using EventStore.ClientAPI;

namespace Athena.EventStore.Projections
{
    public class RunProjections : AppFunctionDefinition
    {
        private bool _running;
        private IEventStoreConnection _connection;
        private readonly IDictionary<string, ProjectionSubscription> _projectionSubscriptions 
            = new Dictionary<string, ProjectionSubscription>();
        
        private readonly ICollection<Transaction> _transactions = new List<Transaction>();
        private readonly ICollection<EventStoreProjection> _projections = new List<EventStoreProjection>();
        private readonly ICollection<MetaDataSupplier> _metaDataSuppliers = new List<MetaDataSupplier>();
        private EventSerializer _serializer = new JsonEventSerializer();
        private EventStoreConnectionString _connectionString 
            = new EventStoreConnectionString("Ip=127.0.0.1;Port=1113;UserName=admin;Password=changeit;");

        private ProjectionsPositionHandler _positionHandler = new StoreProjectionsPositionOnDisc();

        public RunProjections HandleTransactionsWith(Transaction transaction)
        {
            _transactions.Add(transaction);

            return this;
        }

        public RunProjections SupplyMetaDataWith(MetaDataSupplier supplier)
        {
            _metaDataSuppliers.Add(supplier);

            return this;
        }

        public RunProjections WithSerializer(EventSerializer serializer)
        {
            _serializer = serializer;

            return this;
        }

        public RunProjections WithConnectionString(string connectionString)
        {
            _connectionString = new EventStoreConnectionString(connectionString);

            return this;
        }

        public RunProjections WithPositionHandler(ProjectionsPositionHandler positionHandler)
        {
            _positionHandler = positionHandler;

            return this;
        }

        public RunProjections WithProjection(EventStoreProjection projection)
        {
            _projections.Add(projection);

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

        internal IReadOnlyCollection<EventStoreProjection> GetProjections()
        {
            return _projections.ToList();
        }

        internal ProjectionsPositionHandler GetPositionHandler()
        {
            return _positionHandler;
        }

        public string Name { get; } = "esprojection";
        
        protected override AppFunctionBuilder DefineDefaultApplication(AppFunctionBuilder builder)
        {
            return builder
                .First("Retry", next => new Retry(next, 5, TimeSpan.FromSeconds(1), "Projection failed").Invoke)
                .ContinueWith("HandleTransactions",
                    next => new HandleTransactions(next, _transactions.ToList()).Invoke,
                    () => _transactions.GetDiagnosticsData())
                .ContinueWith("SupplyMetaData", next => new SupplyMetaData(next, _metaDataSuppliers.ToList()).Invoke,
                    () => _metaDataSuppliers.GetDiagnosticsData())
                .Last("ExecuteProjection", next => new ExecuteProjection(next).Invoke);
        }

        public async Task Start(AthenaContext context)
        {
            if(_running)
                return;
            
            Logger.Write(LogLevel.Debug, "Starting projections");
            
            _running = true;

            _connection = GetConnectionString().CreateConnection(x => x
                .KeepReconnecting()
                .KeepRetrying()
                .UseCustomLogger(new EventStoreLog()));

            foreach (var projection in GetProjections())
                await StartProjection(projection, context).ConfigureAwait(false);
        }

        public Task Stop()
        {
            Logger.Write(LogLevel.Debug, "Stopping projections");
            
            _running = false;

            foreach (var subscription in _projectionSubscriptions)
                subscription.Value.Close();
            
            _projectionSubscriptions.Clear();
            
            _connection.Close();
            _connection.Dispose();
            _connection = null;
            
            return Task.CompletedTask;
        }

        private async Task StartProjection(EventStoreProjection projection, AthenaContext context)
        {
            await ProjectionInstaller.InstallProjectionFor(projection, GetConnectionString())
                .ConfigureAwait(false);
            
            while (true)
            {
                if(!_running)
                    return;
                
                Logger.Write(LogLevel.Debug, $"Starting projections {projection}");
                
                if (_projectionSubscriptions.ContainsKey(projection.Name))
                {
                    _projectionSubscriptions[projection.Name].Close();
                    _projectionSubscriptions.Remove(projection.Name);
                }

                try
                {
                    var positionHandler = GetPositionHandler();
                    
                    var messageProcessor = new MessageProcessor();
                    
                    var messageSubscription = Observable
                        .FromEvent<DeSerializationResult>(
                            x => messageProcessor.MessageArrived += x, x => messageProcessor.MessageArrived -= x)
                        .Buffer(TimeSpan.FromSeconds(1), 5000)
                        .Select(async x => await HandleEvents(projection, x,
                                y => messageProcessor.OnMessageHandled(y.OriginalEvent.OriginalEventNumber), context)
                            .ConfigureAwait(false))
                        .Subscribe();

                    var messageHandlerSubscription = Observable
                        .FromEvent<long>(
                            x => messageProcessor.MessageHandled += x, x => messageProcessor.MessageHandled -= x)
                        .Buffer(TimeSpan.FromMinutes(1))
                        .Select(async x => await positionHandler.SetLastEvent(projection.Name, x.Max())
                            .ConfigureAwait(false))
                        .Subscribe();
                
                    var eventStoreSubscription = _connection.SubscribeToStreamFrom(projection.Name, 
                        await positionHandler.GetLastEvent(projection.Name).ConfigureAwait(false), 
                        CatchUpSubscriptionSettings.Default,
                        (subscription, evnt) => messageProcessor.OnMessageArrived(GetSerializer().DeSerialize(evnt)),
                        subscriptionDropped: async (subscription, reason, exception) => 
                            await SubscriptionDropped(projection, reason, exception, context).ConfigureAwait(false));

                    _projectionSubscriptions[projection.Name] 
                        = new ProjectionSubscription(messageSubscription, messageHandlerSubscription, eventStoreSubscription);

                    return;
                }
                catch (Exception e)
                {
                    if(!_running)
                        return;
                    
                    Logger.Write(LogLevel.Error, 
                        $"Failed to start projection: {projection}. Retrying...", e);

                    await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
            }
        }
        
        private async Task SubscriptionDropped(EventStoreProjection projection, SubscriptionDropReason reason, 
            Exception exception, AthenaContext context)
        {
            if (!_running)
                return;

            if (reason != SubscriptionDropReason.UserInitiated)
            {
                Logger.Write(LogLevel.Error, $"Projection {projection} failed. Restarting...",
                    exception);
                
                await StartProjection(projection, context).ConfigureAwait(false);
            }
        }
        
        private void StopProjection(EventStoreProjection projection)
        {
            if (!_projectionSubscriptions.ContainsKey(projection.Name))
                return;

            _projectionSubscriptions[projection.Name].Close();
            _projectionSubscriptions.Remove(projection.Name);
            
            Logger.Write(LogLevel.Info, $"Projection {projection} stopped.");
        }
        
        private async Task HandleEvents(EventStoreProjection projection, IEnumerable<DeSerializationResult> events,
            Action<DeSerializationResult> handled, AthenaContext context)
        {
            if (!_running)
                return;

            var eventsList = events.ToList();

            var successfullEvents = eventsList
                .Where(x => x.Successful)
                .ToList();

            if (!successfullEvents.Any())
                return;
            
            var requestEnvironment = new Dictionary<string, object>
            {
                ["context"] = new ProjectionContext(projection, eventsList, handled)
            };

            using (requestEnvironment.EnterApplication(context, "runprojections"))
            {
                try
                {
                    Logger.Write(LogLevel.Debug,
                        $"Execution projections application {Name} for {projection}");

                    await context.Execute(Name, requestEnvironment).ConfigureAwait(false);
                
                    Logger.Write(LogLevel.Debug,
                        $"Projection, \"{projection.Name}\", executed. Application {Name}");
                }
                catch (Exception ex)
                {
                    Logger.Write(LogLevel.Error, 
                        $"Projection {projection.GetType().FullName} has failing handlers. Stopping...", ex);
                
                    StopProjection(projection);
                
                    EventPublishing.Publish(new ProjectionFailed(projection.GetType(), ex), requestEnvironment);
                }   
            }
        }
    }
}