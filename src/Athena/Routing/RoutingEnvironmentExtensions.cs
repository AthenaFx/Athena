using System.Collections.Generic;

namespace Athena.Routing
{
    public static class RoutingEnvironmentExtensions
    {
        public static RouterResult GetRouteResult(this IDictionary<string, object> environment)
        {
            return environment.Get<RouterResult>("router-result");
        }

        internal static void SetRouteResult(this IDictionary<string, object> environment, RouterResult result)
        {
            environment["router-result"] = result;
        }
    }
}