using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web.Routing
{
    public class UrlPatternRouter : EnvironmentRouter
    {
        private readonly IReadOnlyCollection<Route> _routes;
        private readonly RoutePatternMatcher _routePatternMatcher;

        public UrlPatternRouter(IReadOnlyCollection<Route> routes, RoutePatternMatcher routePatternMatcher)
        {
            _routes = routes;
            _routePatternMatcher = routePatternMatcher;
        }

        public Task<RouterResult> Route(IDictionary<string, object> environment)
        {
            var path = environment.GetRequest().Uri.PathAndQuery;
            var httpMethod = environment.GetRequest().Method.ToUpper();

            var route = _routes
                .Where(x => !x.AvailableHttpMethods.Any()
                            || x.AvailableHttpMethods.Any(y => y.Equals(httpMethod, StringComparison.OrdinalIgnoreCase)))
                .Select(x => new
                {
                    Route = x,
                    Match = _routePatternMatcher.Match(path, x.Pattern)
                })
                .FirstOrDefault(x => x.Match.IsMatch);

            return Task.FromResult<RouterResult>(route?.Route?.Destination != null ? new MethodResourceRouterResult(
                route.Route.Destination, route.Match.Parameters) : null);
        }
    }
}