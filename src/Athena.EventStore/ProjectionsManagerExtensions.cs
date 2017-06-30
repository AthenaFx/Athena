using System;
using System.Threading.Tasks;
using Athena.Logging;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;

namespace Athena.EventStore
{
    public static class ProjectionsManagerExtensions
    {
        public static async Task CreateOrUpdateContinuousQueryAsync(this ProjectionsManager projectionsManager, 
            string name, string query, UserCredentials credentials = null)
        {
            var currentProjection = "";
            try
            {
                currentProjection = await projectionsManager.GetQueryAsync(name, credentials).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Write(LogLevel.Info, $"Failed getting es projection \"{name}\"", ex);
            }

            if (string.IsNullOrEmpty(currentProjection))
                await projectionsManager.CreateContinuousAsync(name, query, credentials).ConfigureAwait(false);
            else if (query != currentProjection)
                await projectionsManager.UpdateQueryAsync(name, query, credentials).ConfigureAwait(false);
        }
    }
}