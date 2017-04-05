using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Routing
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExecuteEndpoint
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<EndpointExecutor> _endpointExecutors;

        public ExecuteEndpoint(AppFunc next, IReadOnlyCollection<EndpointExecutor> endpointExecutors)
        {
            _next = next;
            _endpointExecutors = endpointExecutors;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var routeResult = environment.Get<RouterResult>("route-result");

            if (routeResult == null)
            {
                await _next(environment);

                return;
            }

            var results = new List<EndpointExecutionResult>();

            foreach (var executor in _endpointExecutors)
            {
                var result = await executor.Execute(routeResult.RouteTo, environment);

                results.Add(result);
            }

            environment["endpointresults"] = results;

            await _next(environment).ConfigureAwait(false);
        }
    }
}