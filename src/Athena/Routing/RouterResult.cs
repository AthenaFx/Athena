using System.Collections.Generic;
using System.Reflection;

namespace Athena.Routing
{
    public class RouterResult
    {
        public RouterResult(bool success, MethodInfo routeTo, IReadOnlyDictionary<string, object> parameters)
        {
            Success = success;
            RouteTo = routeTo;
            Parameters = parameters;
        }

        public bool Success { get; }
        public MethodInfo RouteTo { get; }
        public IReadOnlyDictionary<string, object> Parameters { get; }
    }
}