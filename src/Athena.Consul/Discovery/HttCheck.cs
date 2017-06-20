using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Messages;
using Athena.PubSub;
using Consul;

namespace Athena.Consul.Discovery
{
    public static class HttCheck
    {
        public static AthenaBootstrapper UseConsulHttpCheck(this AthenaBootstrapper bootstrapper, TimeSpan interval, 
            Uri url)
        {
            var id = $"{Environment.MachineName.ToLower()}-{bootstrapper.ApplicationName.ToLower()}";

            return bootstrapper.UseConsulHttpCheck(interval, url, id);
        }

        public static AthenaBootstrapper UseConsulHttpCheck(this AthenaBootstrapper bootstrapper, TimeSpan interval,
            Uri url, string id)
        {
            return bootstrapper.UseConsulHttpCheck(interval, url, id, "", 0);
        }
        
        public static AthenaBootstrapper UseConsulHttpCheck(this AthenaBootstrapper bootstrapper, TimeSpan interval,
            Uri url, string id, string address, int port)
        {
            return bootstrapper.UseConsulHttpCheck(interval, url, id, address, port, Enumerable.Empty<string>());
        }
        
        public static AthenaBootstrapper UseConsulHttpCheck(this AthenaBootstrapper bootstrapper, TimeSpan interval, 
            Uri url, string id, string address, int port, IEnumerable<string> tags)
        {
            var checkId = $"service:{id}:http";

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
                    Name = $"Service '{bootstrapper.ApplicationName}' http check",
                    ID = checkId,
                    Status = HealthStatus.Passing,
                    HTTP = url.ToString()
                });
            });

            return bootstrapper;
        }
    }
}