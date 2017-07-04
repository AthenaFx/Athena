using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Routing;

namespace Athena.Web.Parsing
{
    public class FindAvailableMediaTypesFromMethodRouteResult : FindMediaTypesForRequest
    {
        private readonly IReadOnlyCollection<EnvironmentDataBinder> _environmentDataBinders;

        public FindAvailableMediaTypesFromMethodRouteResult(
            IReadOnlyCollection<EnvironmentDataBinder> environmentDataBinders)
        {
            _environmentDataBinders = environmentDataBinders;
        }

        public async Task<IReadOnlyCollection<string>> FindAvailableFor(IDictionary<string, object> environment)
        {
            var methodRouterResult = environment.GetRouteResult() as MethodResourceRouterResult;

            if(methodRouterResult == null)
                return new List<string>();

            return ((await ExecuteMethod($"FindAvailableMediaTypesFor{methodRouterResult.Method.Name}",
                        methodRouterResult.Instance, environment, new List<string>()).ConfigureAwait(false)) 
                    as IEnumerable<string> ?? new List<string>{"*/*"})
                .ToList();
        }

        protected virtual async Task<object> ExecuteMethod(string methodName, object instance,
            IDictionary<string, object> environment, object defaultValue)
        {
            var methodInfo = instance
                .GetType()
                .GetMethods()
                .FirstOrDefault(x => x.Name == methodName);

            if(methodInfo == null)
                return defaultValue;

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

            return await (Task<object>) GetType()
                .GetMethod("HandleAsync", new[] {taskResult.GetType()})
                .MakeGenericMethod(returnType)
                .Invoke(this, new object[] {taskResult});
        }

        protected async Task<object> HandleAsync<T>(Task<T> task)
        {
            return await task.ConfigureAwait(false);
        }
    }
}