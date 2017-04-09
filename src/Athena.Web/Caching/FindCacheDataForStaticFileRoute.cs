using System.Threading.Tasks;
using Athena.Routing;
using Athena.Web.Routing;

namespace Athena.Web.Caching
{
    public class FindCacheDataForStaticFileRoute : FindCacheDataForRoute
    {
        public Task<CacheData> Find(RouterResult routeResult)
        {
            var staticFileResource = routeResult as StaticFileRouterResult;

            return Task.FromResult(staticFileResource?.CacheData);
        }
    }
}