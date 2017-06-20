using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Messages;
using Athena.Processes;
using Athena.PubSub;
using Consul;

namespace Athena.Consul.Discovery
{
    public static class TtlCheck
    {
        public static AthenaBootstrapper UseConsulTtlCheck(this AthenaBootstrapper bootstrapper, TimeSpan interval)
        {
            var id = $"{Environment.MachineName.ToLower()}-{bootstrapper.ApplicationName.ToLower()}";

            return bootstrapper.UseConsulTtlCheck(interval, id);
        }

        public static AthenaBootstrapper UseConsulTtlCheck(this AthenaBootstrapper bootstrapper, TimeSpan interval,
            string id)
        {
            return bootstrapper.UseConsulTtlCheck(interval, id, "", 0);
        }
        
        public static AthenaBootstrapper UseConsulTtlCheck(this AthenaBootstrapper bootstrapper, TimeSpan interval,
            string id, string address, int port)
        {
            return bootstrapper.UseConsulTtlCheck(interval, id, address, port, Enumerable.Empty<string>());
        }
        
        public static AthenaBootstrapper UseConsulTtlCheck(this AthenaBootstrapper bootstrapper, TimeSpan interval,
            string id, string address, int port, IEnumerable<string> tags)
        {
            var checkId = $"service:{id}:ttl";

            EventPublishing.Subscribe<BootstrapCompleted>(async x =>
            {
                var client = new ConsulClient();
                
                await client.Agent.ServiceRegister(new AgentServiceRegistration
                {
                    Name = bootstrapper.ApplicationName,
                    ID = id,
                    Address = address,
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

            return bootstrapper
                .UseProcess(new SendTtlDataToConsul(interval - new TimeSpan(interval.Ticks / 2), checkId));
        }
    }
}