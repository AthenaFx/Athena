using Consul;

namespace Athena.Consul.Consensus
{
    public class ConsulLeaderElectionSettings
    {
        public ConsulClient Client { get; private set; } = new ConsulClient();
        public string Name { get; private set; }

        public ConsulLeaderElectionSettings UsingClient(ConsulClient client)
        {
            Client = client;

            return this;
        }

        public ConsulLeaderElectionSettings WithName(string name)
        {
            Name = name;

            return this;
        }
    }
}