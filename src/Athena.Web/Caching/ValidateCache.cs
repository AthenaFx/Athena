using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            var cacheData = await ((Task<CacheData>) GetType()
                .GetMethod("FindCacheDataFor")
                .MakeGenericMethod(routerResult.GetType())
                .Invoke(this, new object[] {routerResult})).ConfigureAwait(false);

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

        protected async Task<CacheData> FindCacheDataFor<TRouteResult>(TRouteResult routeResult)
            where TRouteResult : RouterResult
        {
            var finder = _findCacheDataForRoutes.OfType<FindCacheDataForRoute<TRouteResult>>().FirstOrDefault();

            if(finder == null)
                return CacheData.NotCachable();

            return (await finder.Find(routeResult).ConfigureAwait(false)) ?? CacheData.NotCachable();
        }
    }
}