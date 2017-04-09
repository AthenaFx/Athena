using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Routing;

namespace Athena.Resources
{
    public class MethodResourceExecutor : ResourceExecutor<MethodResourceRouterResult>
    {
        private readonly IReadOnlyCollection<EnvironmentDataBinder> _environmentDataBinders;
        private readonly Func<Type, object> _getInstance;

        public MethodResourceExecutor(IReadOnlyCollection<EnvironmentDataBinder> environmentDataBinders,
            Func<Type, object> getInstance = null)
        {
            _environmentDataBinders = environmentDataBinders;
            _getInstance = getInstance ?? Activator.CreateInstance;
        }

        public async Task<object> Execute(MethodResourceRouterResult resource, IDictionary<string, object> environment)
        {
            var instance = _getInstance(resource.Method.DeclaringType);

            return await Execute(resource.Method, instance, environment).ConfigureAwait(false);
        }

        protected virtual async Task<object> Execute(MethodInfo endpoint, object instance,
            IDictionary<string, object> environment)
        {
            return await ExecuteMethod(endpoint, instance, environment).ConfigureAwait(false);
        }

        protected virtual async Task<object> ExecuteMethod(MethodInfo methodInfo, object instance,
            IDictionary<string, object> environment)
        {
            var parameters = methodInfo.GetParameters();
            var methodArguments = new List<object>();

            foreach (var parameter in parameters)
            {
                methodArguments.Add(await _environmentDataBinders.Bind(parameter.ParameterType, environment)
                    .ConfigureAwait(false));
            }

            var methodResult = methodInfo.Invoke(instance, methodArguments.ToArray());

            var taskResult = methodResult as Task;

            if (taskResult == null)
                return methodResult;

            if (taskResult.GetType() == typeof(Task))
            {
                await taskResult.ConfigureAwait(false);

                return null;
            }

            var returnType = taskResult.GetType().GetGenericArguments()[0];

            var result = await (Task<object>) GetType()
                .GetMethod("HandleAsync", new[] {taskResult.GetType()})
                .MakeGenericMethod(returnType)
                .Invoke(this, new object[] {taskResult});

            return result;
        }

        protected async Task<object> HandleAsync<T>(Task<T> task)
        {
            return await task.ConfigureAwait(false);
        }
    }
}