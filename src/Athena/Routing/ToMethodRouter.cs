using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena.Routing
{
    public abstract class ToMethodRouter : EnvironmentRouter
    {
        private readonly IReadOnlyCollection<MethodInfo> _availableMethods;

        protected ToMethodRouter(IReadOnlyCollection<MethodInfo> availableMethods)
        {
            _availableMethods = availableMethods;
        }

        public Task<RouterResult> Route(IDictionary<string, object> environment)
        {
            var method = Route(environment, _availableMethods);

            return Task.FromResult<RouterResult>(method == null ? null : new MethodResourceRouterResult(method, new Dictionary<string, object>()));
        }

        protected abstract MethodInfo Route(IDictionary<string, object> environment,
            IReadOnlyCollection<MethodInfo> availableMethods);
    }
}