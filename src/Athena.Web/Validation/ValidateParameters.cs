using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Logging;
using Athena.Routing;

namespace Athena.Web.Validation
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ValidateParameters
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<ValidateRouteResult> _validators;
        private readonly AppFunc _onInvalid;

        public ValidateParameters(AppFunc next, IReadOnlyCollection<ValidateRouteResult> validators, 
            AppFunc onInvalid = null)
        {
            _next = next;
            _validators = validators;
            _onInvalid = onInvalid ?? (e =>
            {
                e.GetResponse().StatusCode = 422;
                
                return Task.CompletedTask;
            });
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            Logger.Write(LogLevel.Debug, $"Validating parameter for request {environment.GetRequestId()}");
            
            var result = environment.GetRouteResult();

            if (result == null)
            {
                await _next(environment).ConfigureAwait(false);

                return;
            }

            var validationResult = new ValidationResult();

            foreach (var validator in _validators)
                validationResult = new ValidationResult(validationResult, await validator.Validate(result, environment));

            environment["validation-result"] = validationResult;

            if (!validationResult.IsValid)
            {
                Logger.Write(LogLevel.Debug, $"Request invalid {environment.GetRequestId()}");
                
                await _onInvalid(environment).ConfigureAwait(false);

                return;
            }
            
            Logger.Write(LogLevel.Debug, $"Request valid {environment.GetRequestId()}");

            await _next(environment).ConfigureAwait(false);
        }
    }
}