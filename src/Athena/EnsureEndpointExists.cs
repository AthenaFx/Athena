using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class EnsureEndpointExists
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<CheckIfResourceExists> _resourceCheckers;
        private readonly AppFunc _onMissing;

        public EnsureEndpointExists(AppFunc next, IReadOnlyCollection<CheckIfResourceExists> resourceCheckers, 
            AppFunc onMissing)
        {
            _next = next;
            _resourceCheckers = resourceCheckers;
            _onMissing = onMissing;
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
                        await _onMissing(environment).ConfigureAwait(false);

                        return;
                    }
                }
            }

            await _next(environment).ConfigureAwait(false);
        }
    }
}