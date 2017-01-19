using System.Collections.Generic;

namespace Athena.Web.Routing
{
    public class RouteMatchResult
    {
        public RouteMatchResult(bool isMatch, IReadOnlyDictionary<string, object> parameters)
        {
            IsMatch = isMatch;
            Parameters = parameters;
        }

        public bool IsMatch { get; }
        public IReadOnlyDictionary<string, object> Parameters { get; }
    }
}