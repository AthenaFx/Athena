using System.Threading.Tasks;
using Athena.Configuration;
using Athena.Processes;

namespace Athena.EventStore.Projections
{
    public static class ProjectionsBootstrapExtensions
    {
        public static PartConfiguration<ProjectionSettings> UseEventStoreProjections(
            this AthenaBootstrapper bootstrapper,
            ProjectionsPositionHandler projectionsPositionHandler)
        {
            return bootstrapper
                .UseProcess(new RunProjections())
                .ConfigureWith<ProjectionSettings>((config, context) =>
                {
                    context.DefineApplication(config.Name, config.GetApplicationBuilder());
                    
                    return Task.CompletedTask;
                });
        }
    }
}