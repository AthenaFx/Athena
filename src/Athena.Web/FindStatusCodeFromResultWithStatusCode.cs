namespace Athena.Web
{
    public class FindStatusCodeFromResultWithStatusCode : FindStatusCodeFromResult
    {
        private readonly int _defaultStatusCode;

        public FindStatusCodeFromResultWithStatusCode(int defaultStatusCode = 200)
        {
            _defaultStatusCode = defaultStatusCode;
        }

        public int FindFor(object result)
        {
            var resultWithStatusCode = result as ResultWithStatusCode;

            return resultWithStatusCode?.GetStatusCode() ?? _defaultStatusCode;
        }
    }
}