using System.Threading.Tasks;

namespace Athena.EventStore.Projections
{
    public class ProjectionInstaller
    {
        public static async Task InstallProjectionFor(EventStoreProjection projection, 
            EventStoreConnectionString connectionString)
        {
            var projectionManager = connectionString.CreateProjectionsManager();
            var credentials = connectionString.GetUserCredentials();
            
            var name = $"project-to-{projection.Name}";
            var query = ProjectionBuilder.BuildStreamProjection(projection.GetStreamsToProjectFrom(), projection.Name);

            await projectionManager.CreateOrUpdateContinuousQueryAsync(name, query, credentials).ConfigureAwait(false);
        }
    }
}