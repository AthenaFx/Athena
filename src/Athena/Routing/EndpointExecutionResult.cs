namespace Athena.Routing
{
    public class EndpointExecutionResult
    {
        public EndpointExecutionResult(bool success, object result)
        {
            Success = success;
            Result = result;
        }

        public bool Success { get; }
        public object Result { get; }
    }
}