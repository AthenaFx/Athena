using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web.Caching
{
    public interface FindCacheDataForRoute
    {
        Task<CacheData> Find(RouterResult routeResult);
    }
}