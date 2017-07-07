using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Routing;

namespace Athena.Authorization
{
    public class MethodRouteConventionalAuthorizer : RouteAuthorizer<MethodResourceRouterResult>
    {
        private readonly IReadOnlyCollection<EnvironmentDataBinder> _environmentDataBinders;
        private static readonly ConcurrentDictionary<Type, MethodInfo> AuthorizeMethods =
            new ConcurrentDictionary<Type, MethodInfo>();
        
        public MethodRouteConventionalAuthorizer(IReadOnlyCollection<EnvironmentDataBinder> environmentDataBinders)
        {
            _environmentDataBinders = environmentDataBinders;
        }

        protected override async Task<AuthorizationResult> Authorize(MethodResourceRouterResult routerResult, 
            AuthenticationIdentity identity, IDictionary<string, object> environment)
        {
            var authorized = await ExecuteMethod($"Authorize{routerResult.Method.Name}",
                routerResult.Instance, environment).ConfigureAwait(false);

            return authorized ? AuthorizationResult.Allowed : AuthorizationResult.Denied;
        }
        
        protected virtual async Task<bool> ExecuteMethod(string methodName, object instance,
            IDictionary<string, object> environment)
        {
            var methodInfo = AuthorizeMethods.GetOrAdd(instance.GetType(), x => x.GetTypeInfo().GetMethods()
                .FirstOrDefault(y => y.Name == methodName
                                     && (y.ReturnType == typeof(bool) || y.ReturnType == typeof(Task<bool>))));

            if(methodInfo == null)
                return true;

            var result = await methodInfo.CompileAndExecute<object>(instance,
                async x => await _environmentDataBinders.Bind(x, environment).ConfigureAwait(false))
                .ConfigureAwait(false);

            var taskResult = result as Task<bool>;

            if (taskResult == null)
                return (bool)result;

            return await taskResult.ConfigureAwait(false);
        }
    }
}