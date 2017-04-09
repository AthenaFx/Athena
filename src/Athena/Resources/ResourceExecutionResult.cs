namespace Athena.Resources
{
    public class ResourceExecutionResult
    {
        public ResourceExecutionResult(bool executed, object result)
        {
            Executed = executed;
            Result = result;
        }

        public bool Executed { get; }
        public object Result { get; }
    }
}