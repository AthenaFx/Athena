using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Athena.EventStore.Serialization;
using Athena.Logging;
using Athena.Processes;
using Athena.PubSub;
using EventStore.ClientAPI;

namespace Athena.EventStore.Projections
{
    public class RunProjections : LongRunningProcess
    {
        private bool _running;
        private IEventStoreConnection _connection;
        private readonly IDictionary<string, ProjectionSubscription> _projectionSubscriptions 
            = new Dictionary<string, ProjectionSubscription>();

        public async Task Start(AthenaContext context)
        {
            if(_running)
                return;
            
            _running = true;

            var settings = context.GetSetting<ProjectionSettings>();

            _connection = settings.GetConnectionString().CreateConnection(x => x
                .KeepReconnecting()
                .KeepRetrying()
                .UseCustomLogger(new EventStoreLog()));

            foreach (var projection in settings.GetProjections())
                await StartProjection(projection, context, settings).ConfigureAwait(false);
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

        private async Task StartProjection(EventStoreProjection projection, AthenaContext context, 
            ProjectionSettings settings)
        {
            await ProjectionInstaller.InstallProjectionFor(projection, settings.GetConnectionString())
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
                    var positionHandler = settings.GetPositionHandler();
                    
                    var messageProcessor = new MessageProcessor();
                    
                    var messageSubscription = Observable
                        .FromEvent<DeSerializationResult>(
                            x => messageProcessor.MessageArrived += x, x => messageProcessor.MessageArrived -= x)
                        .Buffer(TimeSpan.FromSeconds(1), 5000)
                        .Select(async x => await HandleEvents(projection, x,
                                y => messageProcessor.OnMessageHandled(y.OriginalEvent.OriginalEventNumber), context,
                                settings)
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
                        (subscription, evnt) => messageProcessor.OnMessageArrived(settings.GetSerializer().DeSerialize(evnt)),
                        subscriptionDropped: async (subscription, reason, exception) => 
                            await SubscriptionDropped(projection, reason, exception, context, settings).ConfigureAwait(false));

                    _projectionSubscriptions[projection.Name] 
                        = new ProjectionSubscription(messageSubscription, messageHandlerSubscription, eventStoreSubscription);

                    return;
                }
                catch (Exception e)
                {
                    if(!_running)
                        return;
                    
                    Logger.Write(LogLevel.Error, 
                        $"Failed to start projection: {projection.GetType().FullName}. Retrying...", e);

                    await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
            }
        }
        
        private async Task SubscriptionDropped(EventStoreProjection projection, SubscriptionDropReason reason, 
            Exception exception, AthenaContext context, ProjectionSettings settings)
        {
            if (!_running)
                return;

            if (reason != SubscriptionDropReason.UserInitiated)
            {
                Logger.Write(LogLevel.Error, $"Projection {projection.GetType().FullName} failed. Restarting...",
                    exception);
                
                await StartProjection(projection, context, settings).ConfigureAwait(false);
            }
        }
        
        private void StopProjection(EventStoreProjection projection)
        {
            if (!_projectionSubscriptions.ContainsKey(projection.Name))
                return;

            _projectionSubscriptions[projection.Name].Close();
            _projectionSubscriptions.Remove(projection.Name);
            
            Logger.Write(LogLevel.Info, $"Projection {projection.GetType().FullName} stopped.");
        }
        
        private async Task HandleEvents(EventStoreProjection projection, IEnumerable<DeSerializationResult> events,
            Action<DeSerializationResult> handled, AthenaContext context, ProjectionSettings settings)
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

                await context.Execute(settings.Name, requestEnvironment).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Write(LogLevel.Error, 
                    $"Projection {projection.GetType().FullName} has failing handlers. Stopping...", ex);
                
                StopProjection(projection);
                
                await context.Publish(new ProjectionFailed(projection.GetType(), ex))
                    .ConfigureAwait(false);
            }
        }
    }
}