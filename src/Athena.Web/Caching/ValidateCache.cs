using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web.Caching
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ValidateCache
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<FindCacheDataForRoute> _findCacheDataForRoutes;

        public ValidateCache(AppFunc next, IReadOnlyCollection<FindCacheDataForRoute> findCacheDataForRoutes)
        {
            _next = next;
            _findCacheDataForRoutes = findCacheDataForRoutes;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var response = environment.GetResponse();

            var routerResult = environment.GetRouteResult();

            if (routerResult == null)
            {
                await _next(environment);

                return;
            }

            var cacheData = (await _findCacheDataForRoutes
                .Select(async x => await x.Find(routerResult).ConfigureAwait(false))
                .FirstOrDefault(x => x != null).ConfigureAwait(false)) ?? CacheData.NotCachable();

            response.Headers.CacheControl = cacheData.CacheControl;
            response.Headers.ETag = cacheData.Etag;

            var validEtag = environment.GetRequest().Headers.GetHeader("If-None-Match");

            if (!string.IsNullOrEmpty(validEtag) && validEtag == cacheData.Etag)
            {
                response.StatusCode = 304;

                return;
            }

            await _next(environment).ConfigureAwait(false);
        }
    }
}