using System.Threading.Tasks;
using Athena.Web.Routing;

namespace Athena.Web.Caching
{
    public class FindCacheDataForStaticFileRoute : FindCacheDataForRoute<StaticFileRouterResult>
    {
        public Task<CacheData> Find(StaticFileRouterResult routeResult)
        {
            return Task.FromResult(routeResult.CacheData);
        }
    }
}