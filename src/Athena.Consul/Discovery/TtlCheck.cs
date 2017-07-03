using System.Linq;
using Athena.Configuration;
using Athena.Logging;
using Consul;
using LogLevel = Athena.Logging.LogLevel;

namespace Athena.Consul.Discovery
{
    public static class TtlCheck
    {
        public static PartConfiguration<SendTtlDataToConsul> UseConsulTtlCheck(this AthenaBootstrapper bootstrapper)
        {
            Logger.Write(LogLevel.Debug, $"Adding consul ttl check");
            
            return bootstrapper
                .Part<SendTtlDataToConsul>()
                .OnStartup(async (config, context) =>
                {
                    Logger.Write(LogLevel.Debug, $"Configuring consul ttl check");

                    await config.Start().ConfigureAwait(false);
                    
                    await config.CLient.Agent.ServiceRegister(new AgentServiceRegistration
                    {
                        Name = config.ApplicationName,
                        ID = config.Id,
                        Address = config.Address,
                        Port = config.Port,
                        Tags = config.Tags.ToArray()
                    }).ConfigureAwait(false);
                
                    await config.CLient.Agent.CheckRegister(new AgentCheckRegistration
                    {
                        ServiceID = config.Id,
                        Name = config.CheckName,
                        ID = config.CheckId,
                        Status = config.InitialStatus,
                        TTL = config.Ttl
                    });
                }).Configure(x => x.WithApplicationName(bootstrapper.ApplicationName));
        }
    }
}