using System;
using System.Threading;
using System.Threading.Tasks;
using Athena.Logging;
using Athena.Processes;
using Consul;
using LogLevel = Athena.Logging.LogLevel;

namespace Athena.Consul.Discovery
{
    public class SendTtlDataToConsul : LongRunningProcess
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ConsulClient _client;
        private readonly TimeSpan _interval;
        private readonly string _checkId;
        
        public SendTtlDataToConsul(TimeSpan interval, string checkId)
        {
            _interval = interval;
            _checkId = checkId;
            //TODO:Configure
            _client = new ConsulClient();
        }
        
        public Task Start()
        {
            StartSendingTtl();
            
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _cancellationTokenSource.Cancel();
            
            return Task.CompletedTask;
        }

        private void StartSendingTtl()
        {
            var cancellationToken = _cancellationTokenSource.Token;
            
            Task.Run(async () => await SendTtl(cancellationToken).ConfigureAwait(false), cancellationToken)
                .ContinueWith(t =>
                {
                    (t.Exception ?? new AggregateException()).Handle(ex => true);
                    
                    Logger.Write(LogLevel.Warn, "Consul ttl send failed", t.Exception);

                    StartSendingTtl();
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task SendTtl(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _client.Agent.PassTTL(_checkId, "Success", cancellationToken).ConfigureAwait(false);

                await Task.Delay(_interval, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}