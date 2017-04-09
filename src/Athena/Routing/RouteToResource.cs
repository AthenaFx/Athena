using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Routing
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RouteToResource
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<EnvironmentRouter> _environmentRouters;
        private readonly Action<IDictionary<string, object>> _onMissing;

        public RouteToResource(AppFunc next, IReadOnlyCollection<EnvironmentRouter> environmentRouters,
            Action<IDictionary<string, object>> onMissing)
        {
            _next = next;
            _environmentRouters = environmentRouters;
            _onMissing = onMissing;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var routeResult = await _environmentRouters.RouteRequest(environment);

            if (routeResult == null)
            {
                _onMissing(environment);

                return;
            }

            environment.SetRouteResult(routeResult);

            await _next(environment);
        }
    }
}