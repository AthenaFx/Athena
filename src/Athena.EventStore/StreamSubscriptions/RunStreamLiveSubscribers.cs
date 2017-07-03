using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.EventStore.Serialization;
using Athena.Logging;
using Athena.Processes;
using EventStore.ClientAPI;

namespace Athena.EventStore.StreamSubscriptions
{
    public class RunStreamLiveSubscribers : LongRunningProcess
    {
        private readonly IDictionary<string, IServiceSubscription> _serviceSubscriptions =
            new Dictionary<string, IServiceSubscription>();

        private bool _running;
        private IEventStoreConnection _connection;

        public async Task Start(AthenaContext context)
        {
            if (_running)
                return;
            
            Logger.Write(LogLevel.Debug, $"Starting EventStore live subscriptions");

            _running = true;

            var settings = context.GetSetting<LiveSubscribersSettings>();

            _connection = settings.GetConnectionString().CreateConnection(x => x
                .KeepReconnecting()
                .KeepRetrying()
                .UseCustomLogger(new EventStoreLog()));

            var streams = settings.GetSubscribedStreams();

            foreach (var stream in streams)
            {
                await SetupSubscription(stream.Item1, stream.Item2, context, settings).ConfigureAwait(false);
            }
        }

        public Task Stop(AthenaContext context)
        {
            Logger.Write(LogLevel.Debug, $"Stopping EventStore live subscriptions");
            
            _running = false;

            foreach (var subscription in _serviceSubscriptions)
                subscription.Value.Close();

            _serviceSubscriptions.Clear();

            _connection.Close();
            _connection.Dispose();
            _connection = null;

            return Task.CompletedTask;
        }

        private async Task SetupSubscription(string stream, int workers, AthenaContext context,
            LiveSubscribersSettings settings)
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
                                => await HandleEvent(settings.GetSerializer().DeSerialize(evnt), settings,
                                        x => Logger.Write(LogLevel.Debug,
                                            $"Successfully handled event: {x.OriginalEvent.Event.EventId} on stream: {stream}"),
                                        (x, exception)
                                            => Logger.Write(LogLevel.Error,
                                                $"Failed handling event: {x.OriginalEvent.Event.EventId} on stream: {stream}",
                                                exception), context)
                                    .ConfigureAwait(false),
                            async (subscription, reason, exception)
                                => await SubscriptionDropped(stream, workers, reason, exception, context, settings)
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
            Exception exception, AthenaContext context, LiveSubscribersSettings settings)
        {
            if (!_running)
                return;

            Logger.Write(LogLevel.Warn, $"Subscription dropped for stream: {stream}. Reason: {reason}. Retrying...",
                exception);

            if (reason != SubscriptionDropReason.UserInitiated)
                await SetupSubscription(stream, workers, context, settings).ConfigureAwait(false);
        }

        private async Task HandleEvent(DeSerializationResult evnt, LiveSubscribersSettings settings,
            Action<DeSerializationResult> done, Action<DeSerializationResult, Exception> error, AthenaContext context)
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
                    $"Executing live subscription application {settings.Name} for event {evnt.Data}");

                await context.Execute(settings.Name, requestEnvironment).ConfigureAwait(false);

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