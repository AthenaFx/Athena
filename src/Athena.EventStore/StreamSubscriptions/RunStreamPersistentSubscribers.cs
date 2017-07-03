using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.EventStore.Serialization;
using Athena.Logging;
using Athena.Processes;
using EventStore.ClientAPI;

namespace Athena.EventStore.StreamSubscriptions
{
    public class RunStreamPersistentSubscribers : LongRunningProcess
    {
        private readonly IDictionary<string, IServiceSubscription> _serviceSubscriptions =
            new Dictionary<string, IServiceSubscription>();

        private bool _running;
        private IEventStoreConnection _connection;

        public async Task Start(AthenaContext context)
        {
            if (_running)
                return;

            _running = true;

            var settings = context.GetSetting<PersistentSubscribersSettings>();

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
            PersistentSubscribersSettings settings)
        {
            //TODO:Handle multiple workers
            var groupName = $"{context.ApplicationName}-{stream}";

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
                        async (subscription, evnt) => await HandleEvent(settings.GetSerializer().DeSerialize(evnt),
                            settings,
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
                            }, context).ConfigureAwait(false), async (subscription, reason, exception)
                            => await SubscriptionDropped(stream, workers, reason, exception, context,
                                    settings)
                                .ConfigureAwait(false), autoAck: false);

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

        private async Task SubscriptionDropped(string stream, int workers, SubscriptionDropReason reason,
            Exception exception, AthenaContext context, PersistentSubscribersSettings settings)
        {
            if (!_running)
                return;

            Logger.Write(LogLevel.Warn, $"Subscription dropped for stream: {stream}. Reason: {reason}. Retrying...",
                exception);

            if (reason != SubscriptionDropReason.UserInitiated)
                await SetupSubscription(stream, workers, context, settings).ConfigureAwait(false);
        }

        private async Task HandleEvent(DeSerializationResult evnt, PersistentSubscribersSettings settings,
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

                await context.Execute(settings.Name, requestEnvironment).ConfigureAwait(false);

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