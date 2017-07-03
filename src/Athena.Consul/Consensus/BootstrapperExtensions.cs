using Athena.Configuration;
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
    }
}