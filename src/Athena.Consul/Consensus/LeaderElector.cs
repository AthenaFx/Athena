using System;
using System.Threading;
using System.Threading.Tasks;
using Athena.Consensus;
using Athena.Logging;
using Athena.Processes;
using Athena.PubSub;
using Consul;
using LogLevel = Athena.Logging.LogLevel;

namespace Athena.Consul.Consensus
{
    public class LeaderElector : LongRunningProcess
    {
        private readonly string _serviceName;
        private CancellationTokenSource _cancellationTokenSource;
        private NodeRole _currentRole = NodeRole.Follower;
        private IDistributedLock _lock;

        public LeaderElector(string serviceName)
        {
            _serviceName = serviceName;
        }

        public Task Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            
            //TODO:Configure
            var client = new ConsulClient();
            
            _lock = client.CreateLock($"service/{_serviceName}/leader");

            StartLeaderElection();
            
            return Task.CompletedTask;
        }

        public Task Stop()
        {
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
                        await EventPublishing.Publish(new NodeRoleTransitioned(NodeRole.Follower)).ConfigureAwait(false);
                        _currentRole = NodeRole.Follower;
                    }

                    await consulLock.Acquire(cancellationToken).ConfigureAwait(false);
                }

                while (!cancellationToken.IsCancellationRequested && consulLock.IsHeld)
                {
                    if (_currentRole != NodeRole.Leader)
                    {
                        await EventPublishing.Publish(new NodeRoleTransitioned(NodeRole.Leader)).ConfigureAwait(false);
                        _currentRole = NodeRole.Leader;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}