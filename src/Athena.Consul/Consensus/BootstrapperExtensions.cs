using Athena.Processes;

namespace Athena.Consul.Consensus
{
    public static class BootstrapperExtensions
    {
        public static AthenaBootstrapper UseConsulConsensus(this AthenaBootstrapper bootstrapper)
        {
            return bootstrapper.UseProcess(new LeaderElector(bootstrapper.ApplicationName));
        }
    }
}