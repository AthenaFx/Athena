using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;
using Athena.Web.Routing;

namespace Athena.Web.Caching
{
    public class FindCacheDataForStaticFileRequest : FindCacheDataForRequest
    {
        public Task<CacheData> Find(IDictionary<string, object> environment)
        {
            var staticFileResource = environment.GetRouteResult() as StaticFileRouterResult;

            return Task.FromResult(staticFileResource?.CacheData);
        }
    }
}