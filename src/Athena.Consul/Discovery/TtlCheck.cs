using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Configuration;
using Athena.Processes;
using Athena.PubSub;
using Consul;

namespace Athena.Consul.Discovery
{
    public static class TtlCheck
    {
        public static AthenaBootstrapper UseConsulTtlCheck(this AthenaBootstrapper bootstrapper, TimeSpan interval,
            string address = null, int port = 0, string id = null, IEnumerable<string> tags = null, 
            Func<ConsulClient> getClient = null)
        {
            id = id ?? $"{Environment.MachineName.ToLower()}-{bootstrapper.ApplicationName.ToLower()}";
            var checkId = $"service:{id}:ttl";
            getClient = getClient ?? (() => new ConsulClient());

            return bootstrapper
                .UseProcess(new SendTtlDataToConsul(interval - new TimeSpan(interval.Ticks / 2), checkId, getClient()))
                .When<BootstrapCompleted>()
                .Do(async (evnt, context) =>
                {
                    var client = getClient();
                
                    await client.Agent.ServiceRegister(new AgentServiceRegistration
                    {
                        Name = bootstrapper.ApplicationName,
                        ID = id,
                        Address = address ?? "",
                        Port = port,
                        Tags = tags.ToArray()
                    }).ConfigureAwait(false);
                
                    await client.Agent.CheckRegister(new AgentCheckRegistration
                    {
                        ServiceID = id,
                        Name = $"Service '{bootstrapper.ApplicationName}' ttl check",
                        ID = checkId,
                        Status = HealthStatus.Passing,
                        TTL = interval
                    });
                });
        }
    }
}