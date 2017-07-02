using System.Text;
using Athena.Web.Validation;

namespace Athena.Web
{
    public class ValidationErrorsResult
    {
        public ValidationErrorsResult(ValidationResult validationResult)
        {
            ValidationResult = validationResult;
        }

        public ValidationResult ValidationResult { get; }

        public override string ToString()
        {
            if (ValidationResult == null)
                return "Invalid";
            
            var builder = new StringBuilder();

            foreach (var error in ValidationResult.Errors)
            {
                builder.Append($"{error.Key}: {error.Message}");
            }

            return builder.ToString();
        }
    }
}