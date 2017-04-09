namespace Athena.Binding
{
    public class DataBinderResult
    {
        public DataBinderResult(object result, bool success)
        {
            Result = result;
            Success = success;
        }

        public object Result { get; }
        public bool Success { get; }
    }
}