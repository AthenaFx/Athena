using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Athena.Binding;
using Athena.Routing;

namespace Athena.Web.Validation
{
    public class ConventionalMethodRouteValidator : ValidateRouteResult
    {
        private readonly IReadOnlyCollection<EnvironmentDataBinder> _environmentDataBinders;

        private static readonly ConcurrentDictionary<MethodInfo, MethodInfo> ValidationMethods =
            new ConcurrentDictionary<MethodInfo, MethodInfo>();
        
        public ConventionalMethodRouteValidator(IReadOnlyCollection<EnvironmentDataBinder> environmentDataBinders)
        {
            _environmentDataBinders = environmentDataBinders;
        }
        
        public async Task<ValidationResult> Validate(RouterResult result, IDictionary<string, object> environment)
        {
            var methodRouterResult = environment.GetRouteResult() as MethodResourceRouterResult;

            if (methodRouterResult == null)
                return new ValidationResult();
            
            return (await ExecuteMethod(methodRouterResult.Method,
                       methodRouterResult.Instance, environment).ConfigureAwait(false)) ?? new ValidationResult();
        }
        
        protected virtual async Task<ValidationResult> ExecuteMethod(MethodInfo routedTo, object instance,
            IDictionary<string, object> environment)
        {
            var methodInfo = ValidationMethods.GetOrAdd(routedTo, x => x.DeclaringType.GetTypeInfo().GetMethods()
                .FirstOrDefault(y => y.Name == $"Validate{x.Name}"
                                     && (y.ReturnType == typeof(ValidationResult) ||
                                         y.ReturnType == typeof(Task<ValidationResult>))));

            if(methodInfo == null)
                return new ValidationResult();

            var result = await methodInfo.CompileAndExecute<object>(instance,
                    async x => await _environmentDataBinders.Bind(x, environment).ConfigureAwait(false))
                .ConfigureAwait(false);

            var taskResult = result as Task<ValidationResult>;

            if (taskResult == null)
                return (ValidationResult)result;

            return await taskResult.ConfigureAwait(false);
        }

    }
}