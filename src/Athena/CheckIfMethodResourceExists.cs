using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Routing;

namespace Athena
{
    public class CheckIfMethodResourceExists : CheckIfResourceExists
    {
        private readonly IReadOnlyCollection<EnvironmentDataBinder> _environmentDataBinders;

        private static readonly ConcurrentDictionary<Type, MethodInfo> ExistsMethods =
            new ConcurrentDictionary<Type, MethodInfo>();

        public CheckIfMethodResourceExists(IReadOnlyCollection<EnvironmentDataBinder> environmentDataBinders)
        {
            _environmentDataBinders = environmentDataBinders;
        }

        public async Task<bool> Exists(RouterResult result, IDictionary<string, object> environment)
        {
            var methodRouterResult = result as MethodResourceRouterResult;

            if(methodRouterResult == null)
                return true;

            return await ExecuteMethod($"{methodRouterResult.Method.Name}Exists",
                methodRouterResult.Instance, environment).ConfigureAwait(false);
        }

        protected virtual async Task<bool> ExecuteMethod(string methodName, object instance,
            IDictionary<string, object> environment)
        {
            var methodInfo = ExistsMethods.GetOrAdd(instance.GetType(), x => x.GetTypeInfo().GetMethods()
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