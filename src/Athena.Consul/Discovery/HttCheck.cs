using System.Linq;
using Athena.Configuration;
using Athena.Logging;
using Consul;
using LogLevel = Athena.Logging.LogLevel;

namespace Athena.Consul.Discovery
{
    public static class HttCheck
    {
        public static AthenaBootstrapper UseConsulHttpCheck(this AthenaBootstrapper bootstrapper, string url)
        {
            Logger.Write(LogLevel.Debug, $"Adding consul http check");
            
            return bootstrapper
                .ConfigureWith<ConsulHttpCheckSettings, BootstrapCompleted>(async (config, evnt, context) =>
                {
                    Logger.Write(LogLevel.Debug, $"Configuring consul http check");
                    
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
                        HTTP = url
                    });
                }).Configure(x => x.WithApplicationName(bootstrapper.ApplicationName));
        }
    }
}