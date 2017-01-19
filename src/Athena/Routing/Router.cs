using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Routing
{
    public static class Router
    {
        public static async Task<RouterResult> RouteRequest(this IEnumerable<EnvironmentRouter> environmentRouters, IDictionary<string, object> environment)
        {
            foreach (var router in environmentRouters)
            {
                var result = await router.Route(environment);

                if (result.Success)
                    return result;
            }

            return new RouterResult(false, null, new Dictionary<string, object>());
        }
    }
}