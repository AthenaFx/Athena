using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.PubSub;
using Logger = Athena.Logging.Logger;
using LogLevel = Athena.Logging.LogLevel;

namespace Athena.Consul.Consensus
{
    public static class BootstrapperExtensions
    {
        public static PartConfiguration<ConsulLeaderElector> UseConsulConsensus(
            this AthenaBootstrapper bootstrapper)
        {
            Logger.Write(LogLevel.Debug, "Enabling consul leader election");

            return bootstrapper
                .Part<ConsulLeaderElector>()
                .OnStartup((item, context) => item.Start())
                .OnShutdown((item, context) => item.Stop())
                .Configure(x => x.WithName(bootstrapper.ApplicationName));
        }

        public static PartConfiguration<TPart> RunOnlyOnNode<TPart>(this PartConfiguration<TPart> config,
            NodeRole role) where TPart : class, new()
        {
            return config.ConditionallyRun((changeStatus, context) =>
            {
                EventPublishing.OpenChannel<NodeRoleTransitioned>()
                    .Select(async evnt =>
                    {
                        var shouldRun = evnt.NewRole == role;

                        await changeStatus(shouldRun).ConfigureAwait(false);
                    }).Subscribe();
                
                var settings = context.GetSetting<ConsulLeaderElector>();
                
                if(settings == null)
                    return Task.CompletedTask;

                var shouldRunInitially = settings.CurrentRole == role;

                changeStatus(shouldRunInitially);
                
                return Task.CompletedTask;
            });
        }
    }
}