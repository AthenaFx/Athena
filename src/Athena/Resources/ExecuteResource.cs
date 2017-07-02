using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Resources
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExecuteResource
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<ResourceExecutor> _resourceExecutors;

        public ExecuteResource(AppFunc next, IReadOnlyCollection<ResourceExecutor> resourceExecutors)
        {
            _next = next;
            _resourceExecutors = resourceExecutors;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var routeResult = environment.GetRouteResult();

            if (routeResult == null)
            {
                await _next(environment).ConfigureAwait(false);

                return;
            }

            foreach (var executor in _resourceExecutors)
            {
                var result = await executor.Execute(routeResult, environment).ConfigureAwait(false);

                if (result.Executed)
                {
                    environment.SetResourceResult(result.Result);

                    break;
                }
            }

            await _next(environment).ConfigureAwait(false);
        }
    }
}