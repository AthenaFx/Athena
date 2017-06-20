using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Athena.EventStore.Serialization;
using Athena.Processes;
using EventStore.ClientAPI;

namespace Athena.EventStore.Projections
{
    public class RunProjections : LongRunningProcess
    {
        private readonly IEnumerable<EventStoreProjection> _projections;
        private readonly EventStoreConnectionString _eventStoreConnectionString;
        private readonly EventSerializer _eventSerializer;
        private readonly ProjectionsPositionHandler _projectionsPositionHandler;
        private readonly AthenaContext _athenaContext;
        private bool _running;
        private IEventStoreConnection _connection;
        private readonly IDictionary<string, ProjectionSubscription> _projectionSubscriptions 
            = new Dictionary<string, ProjectionSubscription>();

        public RunProjections(IEnumerable<EventStoreProjection> projections,
            EventStoreConnectionString eventStoreConnectionString, ProjectionsPositionHandler projectionsPositionHandler, 
            AthenaContext athenaContext, EventSerializer eventSerializer = null)
        {
            _projections = projections;
            _eventStoreConnectionString = eventStoreConnectionString;
            _projectionsPositionHandler = projectionsPositionHandler;
            _athenaContext = athenaContext;
            _eventSerializer = eventSerializer ?? new JsonEventSerializer();
        }

        public async Task Start()
        {
            if(_running)
                return;
            
            _running = true;

            _connection = _eventStoreConnectionString.CreateConnection(x => x
                .KeepReconnecting()
                .KeepRetrying()
                .UseCustomLogger(new EventStoreLog()));

            foreach (var projection in _projections)
                await StartProjection(projection).ConfigureAwait(false);
        }

        public Task Stop()
        {
            _running = false;

            foreach (var subscription in _projectionSubscriptions)
                subscription.Value.Close();
            
            _projectionSubscriptions.Clear();
            
            _connection.Close();
            _connection.Dispose();
            _connection = null;
            
            return Task.CompletedTask;
        }

        private async Task StartProjection(EventStoreProjection projection)
        {
            await ProjectionInstaller.InstallProjectionFor(projection, _eventStoreConnectionString)
                .ConfigureAwait(false);
            
            while (true)
            {
                if(!_running)
                    return;
                
                if (_projectionSubscriptions.ContainsKey(projection.Name))
                {
                    _projectionSubscriptions[projection.Name].Close();
                    _projectionSubscriptions.Remove(projection.Name);
                }

                try
                {
                    var messageProcessor = new MessageProcessor();
                    
                    var messageSubscription = Observable
                        .FromEvent<DeSerializationResult>(
                            x => messageProcessor.MessageArrived += x, x => messageProcessor.MessageArrived -= x)
                        .Buffer(TimeSpan.FromSeconds(1), 5000)
                        .Subscribe(async x => await HandleEvents(projection, x,
                                y => messageProcessor.OnMessageHandled(y.OriginalEvent.OriginalEventNumber))
                            .ConfigureAwait(false));

                    var messageHandlerSubscription = Observable
                        .FromEvent<long>(
                            x => messageProcessor.MessageHandled += x, x => messageProcessor.MessageHandled -= x)
                        .Buffer(TimeSpan.FromMinutes(1))
                        .Subscribe(async x => await _projectionsPositionHandler.SetLastEvent(projection.Name, x.Max())
                            .ConfigureAwait(false));
                
                    var eventStoreSubscription = _connection.SubscribeToStreamFrom(projection.Name, 
                        await _projectionsPositionHandler.GetLastEvent(projection.Name).ConfigureAwait(false), 
                        CatchUpSubscriptionSettings.Default,
                        (subscription, evnt) => messageProcessor.OnMessageArrived(_eventSerializer.DeSerialize(evnt)),
                        subscriptionDropped: async (subscription, reason, exception) => 
                            await SubscriptionDropped(projection, reason, exception).ConfigureAwait(false));

                    _projectionSubscriptions[projection.Name] 
                        = new ProjectionSubscription(messageSubscription, messageHandlerSubscription, eventStoreSubscription);

                    return;
                }
                catch (Exception e)
                {
                    if(!_running)
                        return;

                    await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
            }
        }
        
        private async Task SubscriptionDropped(EventStoreProjection projection, SubscriptionDropReason reason, 
            Exception exception)
        {
            if (!_running)
                return;
            
            //TODO:Publish internal event saying the projection failed

            if (reason != SubscriptionDropReason.UserInitiated)
                await StartProjection(projection).ConfigureAwait(false);
        }
        
        private void StopProjection(EventStoreProjection projection)
        {
            if (!_projectionSubscriptions.ContainsKey(projection.Name))
                return;

            _projectionSubscriptions[projection.Name].Close();
            _projectionSubscriptions.Remove(projection.Name);
        }
        
        private async Task HandleEvents(EventStoreProjection projection, IEnumerable<DeSerializationResult> events,
            Action<DeSerializationResult> handled)
        {
            if (!_running)
                return;

            var eventsList = events.ToList();

            var successfullEvents = eventsList
                .Where(x => x.Successful)
                .ToList();

            if (!successfullEvents.Any())
                return;

            try
            {
                var requestEnvironment = new Dictionary<string, object>
                {
                    ["context"] = new ProjectionContext(projection, eventsList, handled)
                };

                await _athenaContext.Execute("esprojection", requestEnvironment).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //TODO:Mark service as failing to we can report that via consul
                StopProjection(projection);
            }
        }
    }
}