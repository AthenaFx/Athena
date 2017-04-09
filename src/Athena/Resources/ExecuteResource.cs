using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            var result = await ((Task<object>) GetType()
                .GetMethod("Execute")
                .MakeGenericMethod(routeResult.GetType())
                .Invoke(this, new object[] {routeResult, environment})).ConfigureAwait(false);

            environment.SetResourceResult(result);

            await _next(environment).ConfigureAwait(false);
        }

        protected async Task<object> Execute<TRouterResult>(TRouterResult routerResult,
            IDictionary<string, object> environment)
            where TRouterResult : RouterResult
        {
            var executor = _resourceExecutors.OfType<ResourceExecutor<TRouterResult>>().FirstOrDefault();

            if (executor == null)
                return null;

            return await executor.Execute(routerResult, environment).ConfigureAwait(false);
        }
    }
}