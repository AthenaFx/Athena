using System.Collections.Generic;
using System.Reflection;

namespace Athena.Routing
{
    public class MethodResourceRouterResult : RouterResult
    {
        public MethodResourceRouterResult(MethodInfo method, object instance, 
            IReadOnlyDictionary<string, object> parameters)
        {
            Method = method;
            Parameters = parameters;
            Instance = instance;
        }

        public MethodInfo Method { get; }
        public object Instance { get; }
        public IReadOnlyDictionary<string, object> Parameters { get; }

        public IReadOnlyDictionary<string, object> GetParameters()
        {
            return Parameters;
        }
    }
}