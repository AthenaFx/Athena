using System.Linq;
using Athena.Configuration;
using Athena.Logging;
using Athena.Processes;
using Consul;
using LogLevel = Athena.Logging.LogLevel;

namespace Athena.Consul.Discovery
{
    public static class TtlCheck
    {
        public static PartConfiguration<ConsulTtlCheckSettings> UseConsulTtlCheck(this AthenaBootstrapper bootstrapper)
        {
            Logger.Write(LogLevel.Debug, $"Adding consul ttl check");
            
            return bootstrapper
                .UseProcess(new SendTtlDataToConsul())
                .ConfigureWith<ConsulTtlCheckSettings, BootstrapCompleted>(async (config, evnt, context) =>
                {
                    Logger.Write(LogLevel.Debug, $"Configuring consul ttl check");
                    
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