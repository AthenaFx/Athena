using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Routing;

namespace Athena.Web.Caching
{
    public class FindCacheDataForMethodEndpoint : FindCacheDataForRequest
    {
        private readonly IReadOnlyCollection<EnvironmentDataBinder> _environmentDataBinders;

        private static readonly ConcurrentDictionary<MethodInfo, MethodInfo> CacheDataMethods =
            new ConcurrentDictionary<MethodInfo, MethodInfo>();
        
        public FindCacheDataForMethodEndpoint(IReadOnlyCollection<EnvironmentDataBinder> environmentDataBinders)
        {
            _environmentDataBinders = environmentDataBinders;
        }
        
        public async Task<CacheData> Find(IDictionary<string, object> environment)
        {
            var methodRouterResult = environment.GetRouteResult() as MethodResourceRouterResult;

            if (methodRouterResult == null)
                return null;
            
            return await ExecuteMethod(methodRouterResult.Method,
                methodRouterResult.Instance, environment).ConfigureAwait(false);
        }
        
        protected virtual async Task<CacheData> ExecuteMethod(MethodInfo routedTo, object instance,
            IDictionary<string, object> environment)
        {
            var methodInfo = CacheDataMethods.GetOrAdd(routedTo, x => x.DeclaringType.GetTypeInfo().GetMethods()
                .FirstOrDefault(y => y.Name == $"GetCacheDataFor{x.Name}"
                                     && (y.ReturnType == typeof(CacheData) || y.ReturnType == typeof(Task<CacheData>))));

            if(methodInfo == null)
                return null;

            var result = await methodInfo.CompileAndExecute<object>(instance,
                    async x => await _environmentDataBinders.Bind(x, environment).ConfigureAwait(false))
                .ConfigureAwait(false);

            var taskResult = result as Task<CacheData>;

            if (taskResult == null)
                return (CacheData)result;

            return await taskResult.ConfigureAwait(false);
        }
    }
}