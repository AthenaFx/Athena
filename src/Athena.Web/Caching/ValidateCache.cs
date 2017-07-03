using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Logging;
using Athena.Routing;

namespace Athena.Web.Caching
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ValidateCache
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<FindCacheDataForRequest> _findCacheDataForRoutes;

        public ValidateCache(AppFunc next, IReadOnlyCollection<FindCacheDataForRequest> findCacheDataForRoutes)
        {
            _next = next;
            _findCacheDataForRoutes = findCacheDataForRoutes;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            Logger.Write(LogLevel.Debug,
                $"Validating cache for {environment.GetRequestId()} ({environment.GetCurrentApplication()}");
            
            var response = environment.GetResponse();

            var cacheData = (await _findCacheDataForRoutes
                .Select(async x => await x.Find(environment).ConfigureAwait(false))
                .FirstOrDefault(x => x != null).ConfigureAwait(false)) ?? CacheData.NotCachable();

            Logger.Write(LogLevel.Debug, $"Setting cache data {cacheData}");
            
            response.Headers.CacheControl = cacheData.CacheControl;
            response.Headers.ETag = cacheData.Etag;

            var validEtag = environment.GetRequest().Headers.GetHeader("If-None-Match");

            if (!string.IsNullOrEmpty(validEtag) && validEtag == cacheData.Etag)
            {
                Logger.Write(LogLevel.Debug, $"Cache ETag matched");
                
                response.StatusCode = 304;

                return;
            }

            await _next(environment).ConfigureAwait(false);
            
            Logger.Write(LogLevel.Debug, $"Cache validated");
        }
    }
}