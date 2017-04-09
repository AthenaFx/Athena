using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Athena.Resources
{
    public class EndpointValidationResult
    {
        public EndpointValidationResult(bool isValid = true,
            IReadOnlyCollection<ValidationMessage> validationMessages = null,
            int? validationStatus = null)
        {
            ValidationStatus = validationStatus;
            IsValid = isValid;
            ValidationMessages = validationMessages
                                 ?? new ReadOnlyCollection<ValidationMessage>(new List<ValidationMessage>());
        }

        public int? ValidationStatus { get; }
        public bool IsValid { get; }
        public IReadOnlyCollection<ValidationMessage> ValidationMessages { get; }

        public class ValidationMessage
        {
            public ValidationMessage(string key, string message)
            {
                Key = key;
                Message = message;
            }

            public string Key { get; }
            public string Message { get; }
        }
    }
}