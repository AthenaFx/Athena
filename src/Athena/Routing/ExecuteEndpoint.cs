using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Athena.Routing
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExecuteEndpoint
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<EnvironmentDataBinder> _environmentDataBinders;
        private readonly Func<EndpointValidationResult, IDictionary<string, object>, Task> _onValidationError;
        private readonly Func<Type, object> _getInstance;

        public ExecuteEndpoint(AppFunc next, IReadOnlyCollection<EnvironmentDataBinder> environmentDataBinders,
            Func<EndpointValidationResult, IDictionary<string, object>, Task> onValidationError = null,
            Func<Type, object> getInstance = null)
        {
            _next = next;
            _environmentDataBinders = environmentDataBinders;
            _onValidationError = onValidationError ?? ((x, y) => Task.CompletedTask);
            _getInstance = getInstance ?? Activator.CreateInstance;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var routeResult = environment.Get<RouterResult>("route-result");

            if (routeResult?.RouteTo == null)
            {
                await _next(environment).ConfigureAwait(false);

                return;
            }

            var instance = _getInstance(routeResult.RouteTo.DeclaringType);

            var validationResult = await Validate(routeResult.RouteTo, instance, environment).ConfigureAwait(false);

            environment["validation-result"] = validationResult;

            if (!validationResult.IsValid)
            {
                await _onValidationError(validationResult, environment).ConfigureAwait(false);

                await _next(environment).ConfigureAwait(false);

                return;
            }

            var result = await Execute(routeResult.RouteTo, instance, environment).ConfigureAwait(false);

            environment["endpointresults"] = result;

            await _next(environment).ConfigureAwait(false);
        }

        protected virtual async Task<EndpointValidationResult> Validate(MethodInfo endpoint, object instance,
            IDictionary<string, object> environment)
        {
            var validationMethod = instance
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(x => x.Name == $"Validate{endpoint.Name}"
                            && (x.ReturnType == typeof(EndpointValidationResult)
                                || x.ReturnType == typeof(Task<EndpointValidationResult>)));

            if(validationMethod == null)
                return new EndpointValidationResult();

            return (await ExecuteMethod(validationMethod, instance, environment).ConfigureAwait(false))
                as EndpointValidationResult ?? new EndpointValidationResult();
        }

        protected virtual async Task<EndpointExecutionResult> Execute(MethodInfo endpoint, object instance,
            IDictionary<string, object> environment)
        {
            var result = await ExecuteMethod(endpoint, instance, environment).ConfigureAwait(false);

            return new EndpointExecutionResult(true, result);
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