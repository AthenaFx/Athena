using System;
using System.Threading;
using System.Threading.Tasks;
using Athena.Logging;
using Athena.Processes;
using LogLevel = Athena.Logging.LogLevel;

namespace Athena.Consul.Discovery
{
    public class SendTtlDataToConsul : LongRunningProcess
    {
        private CancellationTokenSource _cancellationTokenSource;

        public Task Start(AthenaContext context)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var settings = context.GetSetting<ConsulTtlCheckSettings>();
            
            StartSendingTtl(settings);
            
            Logger.Write(LogLevel.Debug, $"Starting consul ttl check {settings.CheckId} for {settings.Id}");
            
            return Task.CompletedTask;
        }

        public Task Stop(AthenaContext context)
        {
            var settings = context.GetSetting<ConsulTtlCheckSettings>();
            
            Logger.Write(LogLevel.Debug, $"Stopping consul ttl check {settings.CheckId} for {settings.Id}");
            
            _cancellationTokenSource?.Cancel();
            
            return Task.CompletedTask;
        }

        private void StartSendingTtl(ConsulTtlCheckSettings settings)
        {
            var cancellationToken = _cancellationTokenSource.Token;
            
            Task.Run(async () => await SendTtl(settings, cancellationToken).ConfigureAwait(false), cancellationToken)
                .ContinueWith(t =>
                {
                    (t.Exception ?? new AggregateException()).Handle(ex => true);
                    
                    Logger.Write(LogLevel.Warn, "Consul ttl send failed", t.Exception);

                    StartSendingTtl(settings);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private static async Task SendTtl(ConsulTtlCheckSettings settings, CancellationToken cancellationToken)
        {
            var interval = settings.Ttl - new TimeSpan(settings.Ttl.Ticks / 2);
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await settings.CLient.Agent.PassTTL(settings.CheckId, "Success", cancellationToken)
                    .ConfigureAwait(false);

                await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}