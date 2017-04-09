using System.Collections.Generic;
using System.Reflection;

namespace Athena.Routing
{
    public class MethodResourceRouterResult : RouterResult
    {
        public MethodResourceRouterResult(MethodInfo method, IReadOnlyDictionary<string, object> parameters)
        {
            Method = method;
            Parameters = parameters;
        }

        public MethodInfo Method { get; }
        public IReadOnlyDictionary<string, object> Parameters { get; }

        public IReadOnlyDictionary<string, object> GetParameters()
        {
            return Parameters;
        }
    }
}