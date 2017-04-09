using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web.Validation
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class EnsureEndpointExists
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<CheckIfResourceExists> _resourceCheckers;

        public EnsureEndpointExists(AppFunc next, IReadOnlyCollection<CheckIfResourceExists> resourceCheckers)
        {
            _next = next;
            _resourceCheckers = resourceCheckers;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var routerResult = environment.GetRouteResult();

            if (routerResult != null)
            {
                foreach (var checker in _resourceCheckers)
                {
                    var exists = await checker.Exists(routerResult, environment).ConfigureAwait(false);

                    if (!exists)
                    {
                        environment.GetResponse().StatusCode = 404;

                        return;
                    }
                }
            }

            await _next(environment).ConfigureAwait(false);
        }
    }
}