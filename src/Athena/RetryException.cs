using System;

namespace Athena
{
    public class RetryException : Exception
    {
        public RetryException(int times, string description, Exception innerException) : base(
            $"Failed {description} after {times} attempts", innerException)
        {
            
        } 
    }
}