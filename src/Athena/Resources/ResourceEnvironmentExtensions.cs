using System.Collections.Generic;

namespace Athena.Resources
{
    public static class ResourceEnvironmentExtensions
    {
        public static object GetResourceResult(this IDictionary<string, object> environment)
        {
            return environment.Get<object>("resource-result");
        }

        public static void SetResourceResult(this IDictionary<string, object> environment, 
            object resourceResult)
        {
            environment["resource-result"] = resourceResult;
        }
    }
}