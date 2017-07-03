using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Athena.Web.Routing
{
    public static class RouteExtensions
    {
        public static IReadOnlyDictionary<string, string> GetDiagnosticsData(this IEnumerable<Route> routes)
        {
            return routes.ToDictionary(x => x.Pattern, x =>
                    $"{x.Destination.DeclaringType.Namespace}.{x.Destination.DeclaringType.Name}.{x.Destination.Name}()");
        }
    }
}