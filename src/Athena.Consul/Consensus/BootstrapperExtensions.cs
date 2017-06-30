using System;
using Athena.Configuration;
using Athena.Processes;
using Consul;

namespace Athena.Consul.Consensus
{
    public static class BootstrapperExtensions
    {
        public static AthenaBootstrapper UseConsulConsensus(this AthenaBootstrapper bootstrapper, 
            Func<ConsulClient> getClient = null)
        {
            getClient = getClient ?? (() => new ConsulClient());
            
            return bootstrapper.UseProcess(new LeaderElector(bootstrapper.ApplicationName, getClient));
        }
    }
}