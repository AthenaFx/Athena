using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Routing
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class FindCorrectRoute
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<EnvironmentRouter> _environmentRouters;

        public FindCorrectRoute(AppFunc next, IReadOnlyCollection<EnvironmentRouter> environmentRouters)
        {
            _next = next;
            _environmentRouters = environmentRouters;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var routeResult = await _environmentRouters.RouteRequest(environment);

            environment["route-result"] = routeResult;

            await _next(environment);
        }
    }
}