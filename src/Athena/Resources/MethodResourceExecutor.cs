﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Routing;

namespace Athena.Resources
{
    public class MethodResourceExecutor : ResourceExecutor
    {
        private readonly IReadOnlyCollection<EnvironmentDataBinder> _environmentDataBinders;
        private readonly Func<Type, object> _getInstance;

        public MethodResourceExecutor(IReadOnlyCollection<EnvironmentDataBinder> environmentDataBinders,
            Func<Type, object> getInstance = null)
        {
            _environmentDataBinders = environmentDataBinders;
            _getInstance = getInstance ?? Activator.CreateInstance;
        }

        public async Task<ResourceExecutionResult> Execute(RouterResult resource, IDictionary<string, object> environment)
        {
            var methodResource = resource as MethodResourceRouterResult;

            if(methodResource == null)
                return new ResourceExecutionResult(false, null);

            var instance = _getInstance(methodResource.Method.DeclaringType);

            var result = await ExecuteMethod(methodResource.Method, instance, environment).ConfigureAwait(false);

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