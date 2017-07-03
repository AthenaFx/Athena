using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Logging;

namespace Athena.Routing
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RouteToResource
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<EnvironmentRouter> _environmentRouters;

        public RouteToResource(AppFunc next, IReadOnlyCollection<EnvironmentRouter> environmentRouters)
        {
            _next = next;
            _environmentRouters = environmentRouters;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            Logger.Write(LogLevel.Debug,
                $"Routing request {environment.GetRequestId()} ({environment.GetCurrentApplication()})");
            
            var routeResult = await _environmentRouters.RouteRequest(environment);

            environment.SetRouteResult(routeResult);

            Logger.Write(LogLevel.Debug,
                $"Request, {environment.GetRequestId()} ({environment.GetCurrentApplication()}), routed {routeResult?.ToString() ?? ""}");

            await _next(environment);
        }
    }
}