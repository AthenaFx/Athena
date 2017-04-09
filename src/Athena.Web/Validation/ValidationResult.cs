using System.Collections.Generic;
using System.Linq;

namespace Athena.Web.Validation
{
    public class ValidationResult
    {
        public ValidationResult() : this(new List<ValidationError>())
        {

        }

        public ValidationResult(IReadOnlyCollection<ValidationError> errors)
        {
            Errors = errors;
        }

        internal ValidationResult(ValidationResult parent, ValidationResult child)
        {
            var newErrors = parent.Errors.ToList();

            if(child != null)
                newErrors.AddRange(child.Errors);

            Errors = newErrors;
        }

        public bool IsValid => !Errors.Any();

        public IReadOnlyCollection<ValidationError> Errors { get; }

        public class ValidationError
        {
            public ValidationError(string key, string message)
            {
                Key = key;
                Message = message;
            }

            public string Key { get; }
            public string Message { get; }
        }
    }
}