using System.Collections.Generic;
using System.Linq;

namespace Athena.Routing
{
    public class MultipleMethodsResourceRouterResult : RouterResult
    {
        public MultipleMethodsResourceRouterResult(IEnumerable<MethodResourceRouterResult> methodRouterResults)
        {
            MethodRouterResults = methodRouterResults;
        }

        public IEnumerable<MethodResourceRouterResult> MethodRouterResults { get; }
        
        public IReadOnlyDictionary<string, object> GetParameters()
        {
            return MethodRouterResults
                .SelectMany(x => x.Parameters)
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Value).FirstOrDefault());
        }
    }
}