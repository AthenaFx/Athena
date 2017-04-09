using System.Collections.Generic;
using Athena.Routing;

namespace Athena.Web.Parsing
{
    public interface FindMediaTypesForRouterResult<in TRouterResult> where TRouterResult : RouterResult
    {
        IReadOnlyCollection<string> FindAvailableFor(TRouterResult routerResult);
    }
}