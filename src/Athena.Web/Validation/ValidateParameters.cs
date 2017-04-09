using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web.Validation
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ValidateParameters
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<ValidateRouteResult> _validators;

        public ValidateParameters(AppFunc next, IReadOnlyCollection<ValidateRouteResult> validators)
        {
            _next = next;
            _validators = validators;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
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
                environment.GetResponse().StatusCode = 422;

                return;
            }

            await _next(environment).ConfigureAwait(false);
        }
    }
}