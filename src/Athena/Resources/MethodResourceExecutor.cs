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
            var parameters = methodInfo.GetParameters();
            var methodArguments = new List<object>();

            foreach (var parameter in parameters)
            {
                methodArguments.Add(await _environmentDataBinders.Bind(parameter.ParameterType, environment)
                    .ConfigureAwait(false));
            }
            
            Logger.Write(LogLevel.Debug, $"Collected {methodArguments.Count} method arguments");

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

            return await (Task<object>) GetType()
                .GetMethod("HandleAsync", BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(returnType)
                .Invoke(this, new object[] {taskResult});
        }

        protected async Task<object> HandleAsync<T>(Task<T> task)
        {
            return await task.ConfigureAwait(false);
        }
    }
}