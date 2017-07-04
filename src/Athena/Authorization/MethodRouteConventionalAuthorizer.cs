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
            //TODO:Cache methods
            var methodInfo = instance
                .GetType()
                .GetMethods()
                .FirstOrDefault(x => x.Name == methodName
                                     && (x.ReturnType == typeof(bool) || x.ReturnType == typeof(Task<bool>)));

            if(methodInfo == null)
                return true;

            var parameters = methodInfo.GetParameters();
            var methodArguments = new List<object>();

            foreach (var parameter in parameters)
            {
                methodArguments.Add(await _environmentDataBinders.Bind(parameter.ParameterType, environment)
                    .ConfigureAwait(false));
            }

            var methodResult = methodInfo.Invoke(instance, methodArguments.ToArray());

            var taskResult = methodResult as Task<bool>;

            if (taskResult == null)
                return (bool)methodResult;

            return await taskResult;
        }
    }
}