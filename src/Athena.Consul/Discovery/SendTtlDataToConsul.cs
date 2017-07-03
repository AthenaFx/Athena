using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Athena.Logging;
using Consul;
using LogLevel = Athena.Logging.LogLevel;

namespace Athena.Consul.Discovery
{
    public class SendTtlDataToConsul
    {
        private CancellationTokenSource _cancellationTokenSource;
        
        private readonly List<string> _tags = new List<string>();
        private string _overrideCheckName;
        private string _overrideId;
        private string _overrideCheckId;
        
        public string ApplicationName { get; private set; }

        public string CheckName => !string.IsNullOrEmpty(_overrideCheckName)
            ? _overrideCheckName
            : $"Service '{ApplicationName}' ttl check";

        public string Id => !string.IsNullOrEmpty(_overrideId)
            ? _overrideId
            : $"{Environment.MachineName.ToLower()}-{ApplicationName.ToLower()}";

        public string CheckId => !string.IsNullOrEmpty(_overrideCheckId)
            ? _overrideCheckId
            : $"service:{Id}:ttl";
        
        public string Address { get; private set; } = "";
        public int Port { get; private set; }
        public IReadOnlyCollection<string> Tags => _tags;
        public HealthStatus InitialStatus { get; private set; } = HealthStatus.Passing;
        public TimeSpan Ttl { get; private set; } = TimeSpan.FromSeconds(30);
        public ConsulClient CLient { get; private set; } = new ConsulClient();

        public SendTtlDataToConsul WithApplicationName(string name)
        {
            ApplicationName = name;

            return this;
        }

        public SendTtlDataToConsul WithCheckName(string name)
        {
            _overrideCheckName = name;

            return this;
        }

        public SendTtlDataToConsul WithId(string id)
        {
            _overrideId = id;

            return this;
        }

        public SendTtlDataToConsul WithCheckId(string id)
        {
            _overrideCheckId = id;

            return this;
        }

        public SendTtlDataToConsul WithAddress(string address)
        {
            Address = address;

            return this;
        }

        public SendTtlDataToConsul WithPort(int port)
        {
            Port = port;

            return this;
        }

        public SendTtlDataToConsul WithInitialStatus(HealthStatus status)
        {
            InitialStatus = status;

            return this;
        }

        public SendTtlDataToConsul WithTtl(TimeSpan ttl)
        {
            Ttl = ttl;

            return this;
        }

        public SendTtlDataToConsul WithClient(ConsulClient client)
        {
            CLient = client;

            return this;
        }

        public SendTtlDataToConsul Tag(string tag)
        {
            _tags.Add(tag);

            return this;
        }

        public Task Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            StartSendingTtl();
            
            Logger.Write(LogLevel.Debug, $"Starting consul ttl check {CheckId} for {Id}");
            
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            Logger.Write(LogLevel.Debug, $"Stopping consul ttl check {CheckId} for {Id}");
            
            _cancellationTokenSource?.Cancel();
            
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
            var interval = Ttl - new TimeSpan(Ttl.Ticks / 2);
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await CLient.Agent.PassTTL(CheckId, "Success", cancellationToken)
                    .ConfigureAwait(false);

                await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}