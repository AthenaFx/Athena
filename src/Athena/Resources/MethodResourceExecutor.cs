using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Logging;
using Athena.Routing;

namespace Athena.Resources
{
    public class MethodResourceExecutor : ResourceExecutor
    {
        private readonly IReadOnlyCollection<EnvironmentDataBinder> _environmentDataBinders;

        private static readonly MethodInfo HandleAsyncMethod = typeof(MethodResourceExecutor).GetTypeInfo()
            .GetMethod("HandleAsync", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly ConcurrentDictionary<Type, MethodInfo> HandleAsyncMethods =
            new ConcurrentDictionary<Type, MethodInfo>();

        public MethodResourceExecutor(IReadOnlyCollection<EnvironmentDataBinder> environmentDataBinders)
        {
            _environmentDataBinders = environmentDataBinders;
        }

        public virtual async Task<ResourceExecutionResult> Execute(RouterResult resource, 
            IDictionary<string, object> environment)
        {
            var methodResource = resource as MethodResourceRouterResult;

            if(methodResource == null)
                return new ResourceExecutionResult(false, null);
            
            Logger.Write(LogLevel.Debug, $"Starting method resource execution {methodResource.Method.Name}");

            var result = await ExecuteMethod(methodResource.Method, methodResource.Instance, environment)
                .ConfigureAwait(false);
            
            Logger.Write(LogLevel.Debug, $"Method resource executed {methodResource.Method.Name}");

            return new ResourceExecutionResult(true, result);
        }
            
        protected virtual async Task<object> ExecuteMethod(MethodInfo methodInfo, object instance,
            IDictionary<string, object> environment)
        {
            var result = await methodInfo.CompileAndExecute<object>(instance,
                    async x => await _environmentDataBinders.Bind(x, environment).ConfigureAwait(false))
                .ConfigureAwait(false);

            var taskResult = result as Task;

            if (taskResult == null)
                return result;

            if (taskResult.GetType() == typeof(Task))
            {
                await taskResult.ConfigureAwait(false);

                return null;
            }

            var returnType = taskResult.GetType().GetTypeInfo().GetGenericArguments()[0];

            return await (await HandleAsyncMethods.GetOrAdd(returnType, x => HandleAsyncMethod.MakeGenericMethod(x))
                .CompileAndExecute<Task<object>>(this, x => Task.FromResult<object>(taskResult))
                .ConfigureAwait(false)).ConfigureAwait(false);
        }

        protected async Task<object> HandleAsync<T>(Task<T> task)
        {
            return await task.ConfigureAwait(false);
        }
    }
}