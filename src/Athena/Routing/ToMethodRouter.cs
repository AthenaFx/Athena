using System;
using System.Collections.Generic;
using System.Linq;
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

            return Task.FromResult<RouterResult>(method == null 
                ? null 
                : new MethodResourceRouterResult(method, CreateInstance(method.DeclaringType, environment), 
                    new Dictionary<string, object>()));
        }

        public IReadOnlyDictionary<string, string> GetAvailableRoutes()
        {
            return _availableMethods
                .Select(GetRouteFor)
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => string.Join(", ", x.Select(y => y.Value)));
        }

        protected abstract MethodInfo Route(IDictionary<string, object> environment,
            IReadOnlyCollection<MethodInfo> availableMethods);

        protected abstract object CreateInstance(Type type, IDictionary<string, object> environment);
        
        protected abstract KeyValuePair<string, string> GetRouteFor(MethodInfo methodInfo);
    }
}