namespace Athena
{
    public class DataBinderResult
    {
        public DataBinderResult(object result, bool success)
        {
            Result = result;
            Success = success;
        }

        public object Result { get; private set; }
        public bool Success { get; private set; }
    }
}