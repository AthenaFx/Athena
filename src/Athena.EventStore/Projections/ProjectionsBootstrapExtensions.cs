using System;
using System.Threading.Tasks;
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
            return bootstrapper
                .UseProcess(new RunProjections())
                .ConfigureWith<ProjectionSettings>((config, context) =>
                {
                    context.DefineApplication("esprojection", config.GetBuilder());
                    
                    return Task.CompletedTask;
                });
        }
        
        public static PartConfiguration<ProjectionSettings> WithSerializer(
            this PartConfiguration<ProjectionSettings> config, EventSerializer serializer)
        {
            return config.UpdateSettings(x => x.WithSerializer(serializer));
        }

        public static PartConfiguration<ProjectionSettings> WithConnectionString(
            this PartConfiguration<ProjectionSettings> config, string connectionString)
        {
            return config.UpdateSettings(x => x.WithConnectionString(connectionString));
        }

        public static PartConfiguration<ProjectionSettings> WithProjection(
            this PartConfiguration<ProjectionSettings> config, EventStoreProjection projection)
        {
            return config.UpdateSettings(x => x.WithProjection(projection));
        }
        
        public static PartConfiguration<ProjectionSettings> ConfigureApplication(
            this PartConfiguration<ProjectionSettings> config, 
            Func<AppFunctionBuilder, AppFunctionBuilder> configure)
        {
            return config.UpdateSettings(x => x.ConfigureApplication(configure));
        }
    }
}