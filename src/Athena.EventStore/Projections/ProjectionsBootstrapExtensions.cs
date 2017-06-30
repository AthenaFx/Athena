using Athena.Configuration;
using Athena.EventStore.Serialization;
using Athena.Processes;

namespace Athena.EventStore.Projections
{
    public static class ProjectionsBootstrapExtensions
    {
        public static PartConfiguration<ProjectionSettings> UseEventStoreProjections(
            this AthenaBootstrapper bootstrapper,
            ProjectionsPositionHandler projectionsPositionHandler)
        {
            var settings = new ProjectionSettings();
            
            bootstrapper =  bootstrapper
                .UsingPlugin(new ProjectionsPlugin())
                .UseProcess(new RunProjections(settings, projectionsPositionHandler));
            
            return new PartConfiguration<ProjectionSettings>(bootstrapper, settings);
        }
        
        public static PartConfiguration<ProjectionSettings> WithSerializer(
            this PartConfiguration<ProjectionSettings> config, EventSerializer serializer)
        {
            return config.ConfigurePart(x => x.WithSerializer(serializer));
        }

        public static PartConfiguration<ProjectionSettings> WithConnectionString(
            this PartConfiguration<ProjectionSettings> config, string connectionString)
        {
            return config.ConfigurePart(x => x.WithConnectionString(connectionString));
        }

        public static PartConfiguration<ProjectionSettings> WithProjection(
            this PartConfiguration<ProjectionSettings> config, EventStoreProjection projection)
        {
            return config.ConfigurePart(x => x.WithProjection(projection));
        }
    }
}