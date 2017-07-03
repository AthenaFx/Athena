using System.Linq;
using System.Threading.Tasks;
using Athena.Configuration;
using Athena.Logging;
using Athena.Processes;

namespace Athena.EventStore.Projections
{
    public static class ProjectionsBootstrapExtensions
    {
        public static PartConfiguration<ProjectionSettings> UseEventStoreProjections(
            this AthenaBootstrapper bootstrapper,
            ProjectionsPositionHandler projectionsPositionHandler)
        {
            Logger.Write(LogLevel.Debug, $"Enabling projections for {bootstrapper.ApplicationName}");
            
            return bootstrapper
                .UseProcess(new RunProjections())
                .ConfigureWith<ProjectionSettings>((config, context) =>
                {
                    var projections = config.GetProjections();

                    Logger.Write(LogLevel.Debug,
                        $"Configuring {projections.Count} projections ({string.Join(", ", projections.Select(x => x.Name))}) for application {context.ApplicationName}");
                    
                    context.DefineApplication(config.Name, config.GetApplicationBuilder());
                    
                    return Task.CompletedTask;
                });
        }
    }
}