namespace Athena.Web
{
    public class StaticStatusCodeFinder : FindStatusCodeFromResult
    {
        private readonly int _statusCode;

        public StaticStatusCodeFinder(int statusCode)
        {
            _statusCode = statusCode;
        }

        public int FindFor(object result)
        {
            return _statusCode;
        }
    }
}