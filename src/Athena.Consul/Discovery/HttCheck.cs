using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Configuration;
using Athena.PubSub;
using Consul;

namespace Athena.Consul.Discovery
{
    public static class HttCheck
    {
        public static AthenaBootstrapper UseConsulHttpCheck(this AthenaBootstrapper bootstrapper, TimeSpan interval, 
            Uri url, string address = null, int port = 0, string id = null, IEnumerable<string> tags = null, 
            Func<ConsulClient> getClient = null)
        {
            id = id ?? $"{Environment.MachineName.ToLower()}-{bootstrapper.ApplicationName.ToLower()}";
            var checkId = $"service:{id}:http";
            getClient = getClient ?? (() => new ConsulClient());

            return bootstrapper
                .When<BootstrapCompleted>()
                .Do(async (evnt, context) =>
                {
                    var client = getClient();
                
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
                        Name = $"Service '{bootstrapper.ApplicationName}' http check",
                        ID = checkId,
                        Status = HealthStatus.Passing,
                        HTTP = url.ToString()
                    });
                });
        }
    }
}