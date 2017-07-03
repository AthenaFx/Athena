using Athena.Configuration;
using Athena.Processes;
using Logger = Athena.Logging.Logger;
using LogLevel = Athena.Logging.LogLevel;

namespace Athena.Consul.Consensus
{
    public static class BootstrapperExtensions
    {
        public static PartConfiguration<ConsulLeaderElectionSettings> UseConsulConsensus(
            this AthenaBootstrapper bootstrapper)
        {
            Logger.Write(LogLevel.Debug, $"Enabling consul leader election");

            return bootstrapper
                .UseProcess(new LeaderElector())
                .ConfigureWith<ConsulLeaderElectionSettings>()
                .Configure(x => x.WithName(bootstrapper.ApplicationName));
        }
    }
}