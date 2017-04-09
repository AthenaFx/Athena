using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Routing;

namespace Athena.Web.Validation
{
    public class CheckIfMethodResourceExists : CheckIfResourceExists
    {
        private readonly IReadOnlyCollection<EnvironmentDataBinder> _environmentDataBinders;
        private readonly Func<Type, object> _getInstance;

        public CheckIfMethodResourceExists(IReadOnlyCollection<EnvironmentDataBinder> environmentDataBinders,
            Func<Type, object> getInstance = null)
        {
            _environmentDataBinders = environmentDataBinders;
            _getInstance = getInstance ?? Activator.CreateInstance;
        }

        public async Task<bool> Exists(RouterResult result, IDictionary<string, object> environment)
        {
            var methodRouterResult = result as MethodResourceRouterResult;

            if(methodRouterResult == null)
                return true;

            var instance = _getInstance(methodRouterResult.Method.DeclaringType);

            return await ExecuteMethod($"{methodRouterResult.Method.Name}Exists",
                instance, environment).ConfigureAwait(false);
        }

        protected virtual async Task<bool> ExecuteMethod(string methodName, object instance,
            IDictionary<string, object> environment)
        {
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