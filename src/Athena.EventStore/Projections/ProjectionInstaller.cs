using System.Threading.Tasks;
using Athena.Logging;

namespace Athena.EventStore.Projections
{
    public class ProjectionInstaller
    {
        public static async Task InstallProjectionFor(EventStoreProjection projection, 
            EventStoreConnectionString connectionString)
        {
            Logger.Write(LogLevel.Debug, $"Installing projections");
            
            var projectionManager = connectionString.CreateProjectionsManager();
            var credentials = connectionString.GetUserCredentials();
            
            var name = $"project-to-{projection.Name}";
            var query = ProjectionBuilder.BuildStreamProjection(projection.GetStreamsToProjectFrom(), projection.Name);

            await projectionManager.CreateOrUpdateContinuousQueryAsync(name, query, credentials).ConfigureAwait(false);
            
            Logger.Write(LogLevel.Debug, $"Projections installed");
        }
    }
}