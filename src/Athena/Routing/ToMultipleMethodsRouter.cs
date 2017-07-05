using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena.Routing
{
    public abstract class ToMultipleMethodsRouter : EnvironmentRouter
    {
        private readonly IReadOnlyCollection<MethodInfo> _availableMethods;
        
        protected ToMultipleMethodsRouter(IReadOnlyCollection<MethodInfo> availableMethods)
        {
            _availableMethods = availableMethods;
        }
        
        public Task<RouterResult> Route(IDictionary<string, object> environment)
        {
            var methods = Route(environment, _availableMethods).ToList();

            var result = methods.Any()
                ? new MultipleMethodsResourceRouterResult(methods
                    .Select(x => new MethodResourceRouterResult(x, CreateInstance(x.DeclaringType, environment),
                        new Dictionary<string, object>())))
                : null;

            return Task.FromResult<RouterResult>(result);
        }

        public IReadOnlyDictionary<string, string> GetAvailableRoutes()
        {
            return _availableMethods
                .Select(GetRouteFor)
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => string.Join(", ", x.Select(y => y.Value)));
        }

        protected abstract IEnumerable<MethodInfo> Route(IDictionary<string, object> environment,
            IReadOnlyCollection<MethodInfo> availableMethods);
        
        protected abstract object CreateInstance(Type type, IDictionary<string, object> environment);

        protected abstract KeyValuePair<string, string> GetRouteFor(MethodInfo methodInfo);
    }
}