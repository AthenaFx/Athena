using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.EventStore.Serialization;
using Athena.Logging;
using Athena.Processes;
using Athena.Settings;
using EventStore.ClientAPI;

namespace Athena.EventStore.StreamSubscriptions
{
    public class RunStreamSubscribers : LongRunningProcess
    {
        private readonly EventStoreConnectionString _eventStoreConnectionString;
        private readonly EventSerializer _eventSerializer;
        private readonly AthenaContext _athenaContext;

        private readonly IDictionary<string, IServiceSubscription> _serviceSubscriptions =
            new Dictionary<string, IServiceSubscription>();

        private bool _running;
        private IEventStoreConnection _connection;

        public RunStreamSubscribers(EventStoreConnectionString eventStoreConnectionString,
            EventSerializer eventSerializer, AthenaContext athenaContext)
        {
            _eventStoreConnectionString = eventStoreConnectionString;
            _eventSerializer = eventSerializer;
            _athenaContext = athenaContext;
        }

        public async Task Start()
        {
            if (_running)
                return;

            _running = true;
            
            _connection = _eventStoreConnectionString.CreateConnection(x => x
                .KeepReconnecting()
                .KeepRetrying()
                .UseCustomLogger(new EventStoreLog()));

            var settings = ApplicationSettings.GetSettings<SubscribersSettings>();

            var streams = settings.GetSubscribedStreams();

            foreach (var stream in streams)
            {
                if (stream.Item3)
                    await SetupLiveSubscription(stream.Item1, stream.Item2).ConfigureAwait(false);
                else
                    await SetupPersistentSubscription(stream.Item1, stream.Item2).ConfigureAwait(false);
            }
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }

        private async Task SetupLiveSubscription(string stream, int workers)
        {
            //TODO:Handle multiple workers
            while (true)
            {
                if (!_running)
                    return;

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
                                => await HandleEvent(_eventSerializer.DeSerialize(evnt), "livesubscription",
                                        x => Logger.Write(LogLevel.Debug,
                                            $"Successfully handled event: {x.OriginalEvent.Event.EventId} on stream: {stream}"),
                                        (x, exception)
                                            => Logger.Write(LogLevel.Error,
                                                $"Failed handling event: {x.OriginalEvent.Event.EventId} on stream: {stream}",
                                                exception))
                                    .ConfigureAwait(false),
                            async (subscription, reason, exception)
                                => await LiveSubscriptionDropped(stream, workers, reason, exception)
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

        private async Task SetupPersistentSubscription(string stream, int workers)
        {
            //TODO:Handle multiple workers
            var groupName = $"{_athenaContext.ApplicationName}-{stream}";

            while (true)
            {
                if (_serviceSubscriptions.ContainsKey(groupName))
                {
                    _serviceSubscriptions[groupName].Close();
                    _serviceSubscriptions.Remove(groupName);
                }

                try
                {
                    var eventstoreSubscription = _connection.ConnectToPersistentSubscription(stream, groupName,
                        async (subscription, evnt) => await HandleEvent(_eventSerializer.DeSerialize(evnt), 
                            "persistentsubscription",
                            x =>
                            {
                                Logger.Write(LogLevel.Debug,
                                    $"Successfully handled event: {x.OriginalEvent.Event.EventId} on stream: {stream}");

                                subscription.Acknowledge(x.OriginalEvent);
                            }, (x, exception) =>
                            {
                                Logger.Write(LogLevel.Error,
                                    $"Failed handling event: {x.OriginalEvent.Event.EventId} on stream: {stream}",
                                    exception);

                                subscription.Fail(x.OriginalEvent, PersistentSubscriptionNakEventAction.Unknown,
                                    exception.Message);
                            }).ConfigureAwait(false), async (subscription, reason, exception) 
                            => await PersistentSubscriptionDropped(stream, workers, reason, exception)
                                .ConfigureAwait(false), autoAck:false);

                    _serviceSubscriptions[groupName] = new PersistentServiceSubscription(eventstoreSubscription);
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
        
        private async Task LiveSubscriptionDropped(string stream, int workers, SubscriptionDropReason reason,
            Exception exception)
        {
            if (!_running)
                return;

            Logger.Write(LogLevel.Warn, $"Subscription dropped for stream: {stream}. Reason: {reason}. Retrying...",
                exception);

            if (reason != SubscriptionDropReason.UserInitiated)
                await SetupLiveSubscription(stream, workers).ConfigureAwait(false);
        }
        
        private async Task PersistentSubscriptionDropped(string stream, int workers, SubscriptionDropReason reason,
            Exception exception)
        {
            if (!_running)
                return;

            Logger.Write(LogLevel.Warn, $"Subscription dropped for stream: {stream}. Reason: {reason}. Retrying...",
                exception);

            if (reason != SubscriptionDropReason.UserInitiated)
                await SetupPersistentSubscription(stream, workers).ConfigureAwait(false);
        }

        private async Task HandleEvent(DeSerializationResult evnt, string applicationType,
            Action<DeSerializationResult> done, Action<DeSerializationResult, Exception> error)
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

                await _athenaContext.Execute(applicationType, requestEnvironment).ConfigureAwait(false);

                done(evnt);
            }
            catch (Exception ex)
            {
                Logger.Write(LogLevel.Error, $"Couldn't handle event: {evnt.Data.GetType().FullName}", ex);
                error(evnt, ex);
            }
        }
    }
}