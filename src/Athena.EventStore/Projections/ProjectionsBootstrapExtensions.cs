using System.Linq;
using Athena.Configuration;
using Athena.Logging;

namespace Athena.EventStore.Projections
{
    public static class ProjectionsBootstrapExtensions
    {
        public static PartConfiguration<RunProjections> UseEventStoreProjections(
            this AthenaBootstrapper bootstrapper,
            ProjectionsPositionHandler projectionsPositionHandler)
        {
            Logger.Write(LogLevel.Debug, $"Enabling projections for {bootstrapper.ApplicationName}");
            
            return bootstrapper
                .Part<RunProjections>()
                .OnSetup(async (config, context) =>
                {
                    var projections = config.GetProjections();

                    Logger.Write(LogLevel.Debug,
                        $"Configuring {projections.Count} projections ({string.Join(", ", projections.Select(x => x.Name))}) for application {context.ApplicationName}");
                    
                    await context.DefineApplication(config.Name, config.GetApplicationBuilder()).ConfigureAwait(false);
                })
                .OnStartup((conf, context) => conf.Start(context))
                .OnShutdown((conf, context) => conf.Stop());
        }
    }
}