using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Logging;
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

            Logger.Write(LogLevel.Debug,
                $"Checking if resource exists for request {environment.GetRequestId()} ({environment.GetCurrentApplication()}) using {_resourceCheckers.Count} checkers ({string.Join(", ", _resourceCheckers.Select(x => x.ToString()))})");

            foreach (var checker in _resourceCheckers)
            {
                var exists = await checker.Exists(routerResult, environment).ConfigureAwait(false);

                if (!exists)
                {
                    Logger.Write(LogLevel.Debug,
                        $"Checker {checker} decided that current resource doesn't exist for request {environment.GetRequestId()} ({environment.GetCurrentApplication()})");
                    
                    await _onMissing(environment).ConfigureAwait(false);

                    return;
                }
            }
            
            Logger.Write(LogLevel.Debug, $"Resource exists");

            await _next(environment).ConfigureAwait(false);
        }
    }
}