using System;
using System.Threading;
using System.Threading.Tasks;
using Athena.Logging;
using Athena.Processes;
using Athena.PubSub;

namespace Athena.ApplicationTimeouts
{
    public class TimeoutManager : LongRunningProcess
    {
        private readonly Func<TimeoutStore> _getTimeoutStore;
        private readonly int _secondsToSleepBetweenPolls;
        private readonly object _lockObject = new object();
        
        private CancellationTokenSource _tokenSource;

        public TimeoutManager(Func<TimeoutStore> getTimeoutStore, int secondsToSleepBetweenPolls)
        {
            _getTimeoutStore = getTimeoutStore;
            _secondsToSleepBetweenPolls = secondsToSleepBetweenPolls;
        }

        public Task Start(AthenaContext context)
        {
            var timeoutStore = _getTimeoutStore();
            
            if(timeoutStore == null)
                return Task.CompletedTask;
            
            _tokenSource = new CancellationTokenSource();

            StartCheckingTimeouts(timeoutStore, context);
            
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _tokenSource?.Cancel();
            
            return Task.CompletedTask;
        }

        private void StartCheckingTimeouts(TimeoutStore timeoutStore, AthenaContext context)
        {
            var cancellationToken = _tokenSource.Token;
            
            Task.Run(async () => await Poll(timeoutStore, cancellationToken, context).ConfigureAwait(false), cancellationToken)
                .ContinueWith(t =>
                {
                    (t.Exception ?? new AggregateException()).Handle(ex => true);
                    
                    Logger.Write(LogLevel.Warn, "Timeout poll failed", t.Exception);

                    StartCheckingTimeouts(timeoutStore, context);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task Poll(TimeoutStore timeoutStore, CancellationToken token, AthenaContext context)
        {
            var startSlice = DateTime.UtcNow.AddYears(-10);
            var nextRetrieval = DateTime.UtcNow;

            while (!token.IsCancellationRequested)
            {
                if (nextRetrieval > DateTime.UtcNow)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_secondsToSleepBetweenPolls), token).ConfigureAwait(false);
                    continue;
                }

                var nextExpiredTimeout = await timeoutStore.GetNextChunk(startSlice, async x =>
                {
                    if (startSlice < x.Item2)
                        startSlice = x.Item2;

                    await context.Publish(x.Item1.Message).ConfigureAwait(false);
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