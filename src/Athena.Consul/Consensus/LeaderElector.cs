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
        private CancellationTokenSource _cancellationTokenSource;
        private NodeRole _currentRole = NodeRole.Follower;
        private IDistributedLock _lock;

        public Task Start(AthenaContext context)
        {
            var settings = context.GetSetting<ConsulLeaderElectionSettings>();
            
            Logger.Write(LogLevel.Debug, $"Starting leader election for {settings.Name}");
            
            _cancellationTokenSource = new CancellationTokenSource();

            var client = settings.Client;
            
            _lock = client.CreateLock($"service/{settings.Name}/leader");

            StartLeaderElection(context, settings);
            
            return Task.CompletedTask;
        }

        public Task Stop(AthenaContext context)
        {
            var settings = context.GetSetting<ConsulLeaderElectionSettings>();
            
            Logger.Write(LogLevel.Debug, $"Stopping leader election for {settings.Name}");
            
            _cancellationTokenSource?.Cancel();

            if(_lock != null && _lock.IsHeld)
                _lock.Release();

            return Task.CompletedTask;
        }
        
        private void StartLeaderElection(AthenaContext context, ConsulLeaderElectionSettings settings)
        {
            var cancellationToken = _cancellationTokenSource.Token;
            
            Task.Run(async () => await AcquireLock(context, _lock, settings, cancellationToken).ConfigureAwait(false), 
                    cancellationToken)
                .ContinueWith(t =>
                {
                    (t.Exception ?? new AggregateException()).Handle(ex => true);
                    
                    Logger.Write(LogLevel.Warn, "Consul leader election failed", t.Exception);

                    StartLeaderElection(context, settings);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
        
        private async Task AcquireLock(AthenaContext context, IDistributedLock consulLock, 
            ConsulLeaderElectionSettings settings, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!consulLock.IsHeld)
                {
                    if (_currentRole != NodeRole.Follower)
                    {
                        Logger.Write(LogLevel.Debug, $"Node became follower of {settings.Name}");
                        
                        await context.Publish(new NodeRoleTransitioned(NodeRole.Follower)).ConfigureAwait(false);
                        _currentRole = NodeRole.Follower;
                    }

                    await consulLock.Acquire(cancellationToken).ConfigureAwait(false);
                }

                while (!cancellationToken.IsCancellationRequested && consulLock.IsHeld)
                {
                    if (_currentRole != NodeRole.Leader)
                    {
                        Logger.Write(LogLevel.Debug, $"Node became leader of {settings.Name}");
                        
                        await context.Publish(new NodeRoleTransitioned(NodeRole.Leader)).ConfigureAwait(false);
                        _currentRole = NodeRole.Leader;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}