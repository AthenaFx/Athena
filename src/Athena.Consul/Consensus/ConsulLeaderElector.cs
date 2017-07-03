using System;
using System.Threading;
using System.Threading.Tasks;
using Athena.Consensus;
using Athena.Logging;
using Athena.PubSub;
using Consul;
using LogLevel = Athena.Logging.LogLevel;

namespace Athena.Consul.Consensus
{
    public class ConsulLeaderElector
    {
        private CancellationTokenSource _cancellationTokenSource;
        private NodeRole _currentRole = NodeRole.Follower;
        private IDistributedLock _lock;
        
        public ConsulClient Client { get; private set; } = new ConsulClient();
        public string Name { get; private set; }

        public ConsulLeaderElector UsingClient(ConsulClient client)
        {
            Client = client;

            return this;
        }

        public ConsulLeaderElector WithName(string name)
        {
            Name = name;

            return this;
        }

        public Task Start()
        {
            Logger.Write(LogLevel.Debug, $"Starting leader election for {Name}");
            
            _cancellationTokenSource = new CancellationTokenSource();
            
            _lock = Client.CreateLock($"service/{Name}/leader");

            StartLeaderElection();
            
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            Logger.Write(LogLevel.Debug, $"Stopping leader election for {Name}");
            
            _cancellationTokenSource?.Cancel();

            if(_lock != null && _lock.IsHeld)
                _lock.Release();

            return Task.CompletedTask;
        }
        
        private void StartLeaderElection()
        {
            var cancellationToken = _cancellationTokenSource.Token;
            
            Task.Run(async () => await AcquireLock(_lock, cancellationToken).ConfigureAwait(false), cancellationToken)
                .ContinueWith(t =>
                {
                    (t.Exception ?? new AggregateException()).Handle(ex => true);
                    
                    Logger.Write(LogLevel.Warn, "Consul leader election failed", t.Exception);

                    StartLeaderElection();
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
        
        private async Task AcquireLock(IDistributedLock consulLock, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!consulLock.IsHeld)
                {
                    if (_currentRole != NodeRole.Follower)
                    {
                        Logger.Write(LogLevel.Debug, $"Node became follower of {Name}");
                        
                        await EventPublishing.Publish(new NodeRoleTransitioned(NodeRole.Follower)).ConfigureAwait(false);
                        _currentRole = NodeRole.Follower;
                    }

                    await consulLock.Acquire(cancellationToken).ConfigureAwait(false);
                }

                while (!cancellationToken.IsCancellationRequested && consulLock.IsHeld)
                {
                    if (_currentRole != NodeRole.Leader)
                    {
                        Logger.Write(LogLevel.Debug, $"Node became leader of {Name}");
                        
                        await EventPublishing.Publish(new NodeRoleTransitioned(NodeRole.Leader)).ConfigureAwait(false);
                        _currentRole = NodeRole.Leader;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}