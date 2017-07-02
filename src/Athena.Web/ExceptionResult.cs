using System;
using System.Text;

namespace Athena.Web
{
    public class ExceptionResult
    {
        public ExceptionResult(Exception exception)
        {
            var messageBuilder = new StringBuilder();

            var currentException = exception;

            while (currentException != null)
            {
                messageBuilder.AppendLine(currentException.Message);
                messageBuilder.AppendLine(currentException.StackTrace);
                
                currentException = currentException.InnerException;
            }

            Message = messageBuilder.ToString();
        }

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}