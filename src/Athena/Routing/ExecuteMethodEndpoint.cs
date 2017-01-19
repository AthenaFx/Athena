using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena.Routing
{
    public class ExecuteMethodEndpoint : EndpointExecutor
    {
        private readonly IReadOnlyCollection<EnvironmentDataBinder> _environmentDataBinders;
        private readonly Func<Type, object> _getInstance;

        public ExecuteMethodEndpoint(IReadOnlyCollection<EnvironmentDataBinder> environmentDataBinders, Func<Type, object> getInstance = null)
        {
            _environmentDataBinders = environmentDataBinders;
            _getInstance = getInstance ?? Activator.CreateInstance;
        }

        public async Task<EndpointExecutionResult> Execute(object endpoint, IDictionary<string, object> environment)
        {
            var methodRouteResult = endpoint as MethodInfo;

            if(methodRouteResult == null)
                return new EndpointExecutionResult(false, null);

            var parameters = methodRouteResult.GetParameters();
            var methodArguments = new List<object>();

            foreach (var parameter in parameters)
                methodArguments.Add(await _environmentDataBinders.Bind(parameter.ParameterType, environment).ConfigureAwait(false));

            var instance = _getInstance(methodRouteResult.DeclaringType);

            var methodResult = methodRouteResult.Invoke(instance, methodArguments.ToArray());

            var taskResult = methodResult as Task;

            if (taskResult != null)
            {
                if (taskResult.GetType() == typeof(Task))
                {
                    await taskResult.ConfigureAwait(false);

                    return new EndpointExecutionResult(true, null);
                }

                var returnType = taskResult.GetType().GetGenericArguments()[0];

                var result = await (Task<object>) GetType()
                    .GetMethod("HandleAsync", new[] {taskResult.GetType()})
                    .MakeGenericMethod(returnType)
                    .Invoke(this, new object[] {taskResult});

                return new EndpointExecutionResult(true, result);
            }

            return new EndpointExecutionResult(true, methodResult);
        }

        protected async Task<object> HandleAsync<T>(Task<T> task)
        {
            return await task.ConfigureAwait(false);
        }
    }
}