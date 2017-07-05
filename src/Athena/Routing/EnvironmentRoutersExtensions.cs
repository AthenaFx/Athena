using System.Collections.Generic;
using System.Linq;

namespace Athena.Routing
{
    public static class EnvironmentRoutersExtensions
    {
        public static IReadOnlyDictionary<string, string> GetDiagnosticsData(
            this IEnumerable<EnvironmentRouter> routers)
        {
            return routers
                .SelectMany(x => x.GetAvailableRoutes())
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => string.Join(", ", x.Select(y => y.Value)));
        }
    }
}