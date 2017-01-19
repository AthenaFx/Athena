﻿using System.Collections.Generic;
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
                .Where(x => !x.AvailableHttpMethods.Any() || x.AvailableHttpMethods.Contains(httpMethod))
                .Select(x => new
                {
                    Route = x,
                    Match = _routePatternMatcher.Match(path, x.Pattern)
                })
                .FirstOrDefault(x => x.Match.IsMatch);

            return Task.FromResult(route == null ? new RouterResult(false, null, new Dictionary<string, object>()) : new RouterResult(true, route.Route.Destination, route.Match.Parameters));
        }
    }
}