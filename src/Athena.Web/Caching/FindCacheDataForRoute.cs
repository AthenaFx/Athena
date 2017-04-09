using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web.Caching
{
    public interface FindCacheDataForRoute
    {

    }

    public interface FindCacheDataForRoute<in TRouteResult> : FindCacheDataForRoute where TRouteResult : RouterResult
    {
        Task<CacheData> Find(TRouteResult routeResult);
    }
}