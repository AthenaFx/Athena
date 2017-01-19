using System.Collections.Generic;

namespace Athena.Routing
{
    public class RouterResult
    {
        public RouterResult(bool success, object routeTo, IReadOnlyDictionary<string, object> parameters)
        {
            Success = success;
            RouteTo = routeTo;
            Parameters = parameters;
        }

        public bool Success { get; }
        public object RouteTo { get; }
        public IReadOnlyDictionary<string, object> Parameters { get; }
    }
}