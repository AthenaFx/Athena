using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Athena.Logging;
using Athena.PubSub;

namespace Athena.ApplicationTimeouts
{
    public class TimeoutManager
    {
        private TimeoutStore _timeoutStore = new NullTimeoutStore();
        private TimeSpan _interval = TimeSpan.FromSeconds(10);
        private readonly object _lockObject = new object();
        
        private CancellationTokenSource _tokenSource;

        public async Task RequestTimeout(object message, DateTime at)
        {
            Logger.Write(LogLevel.Debug, $"Requesting timeout at {at} ({message.GetType()})");
            
            await _timeoutStore.Add(new TimeoutData(Guid.NewGuid(), message, at)).ConfigureAwait(false);
        }

        public TimeoutManager WithStore(TimeoutStore store)
        {
            _timeoutStore = store;

            return this;
        }

        public TimeoutManager WithMaxPollingInterval(TimeSpan interval)
        {
            _interval = interval;

            return this;
        }

        public Task Start(AthenaContext context)
        {            
            if(_timeoutStore == null)
                return Task.CompletedTask;

            Logger.Write(LogLevel.Debug, "Starting timeout manager");
            
            _tokenSource = new CancellationTokenSource();

            StartCheckingTimeouts(context);
            
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _tokenSource?.Cancel();
            
            return Task.CompletedTask;
        }

        private void StartCheckingTimeouts(AthenaContext context)
        {
            var cancellationToken = _tokenSource.Token;
            
            Task.Run(async () => await Poll(cancellationToken, context).ConfigureAwait(false), cancellationToken)
                .ContinueWith(t =>
                {
                    (t.Exception ?? new AggregateException()).Handle(ex => true);
                    
                    Logger.Write(LogLevel.Warn, "Timeout poll failed", t.Exception);

                    StartCheckingTimeouts(context);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task Poll(CancellationToken token, AthenaContext context)
        {
            var startSlice = DateTime.UtcNow.AddYears(-10);
            var nextRetrieval = DateTime.UtcNow;

            while (!token.IsCancellationRequested)
            {
                var environment = new Dictionary<string, object>();
                using (environment.EnterApplication(context, "timeoutmanager"))
                {
                    Logger.Write(LogLevel.Debug, "Polling for timeouts");
                
                    if (nextRetrieval > DateTime.UtcNow)
                    {
                        await Task.Delay(_interval, token).ConfigureAwait(false);
                        continue;
                    }

                    var nextExpiredTimeout = await _timeoutStore.GetNextChunk(startSlice, x =>
                    {
                        if (startSlice < x.Item2)
                            startSlice = x.Item2;

                        EventPublishing.Publish(x.Item1.Message, environment);
                    
                        return Task.CompletedTask;
                    }).ConfigureAwait(false);

                    nextRetrieval = nextExpiredTimeout;

                    lock (_lockObject)
                    {
                        if (nextExpiredTimeout < nextRetrieval)
                            nextRetrieval = nextExpiredTimeout;
                    }

                    var maxNextRetrieval = DateTime.UtcNow + TimeSpan.FromMinutes(1);

                    if (nextRetrieval > maxNextRetrieval)
                        nextRetrieval = maxNextRetrieval;   
                }
            }
        }
    }
}