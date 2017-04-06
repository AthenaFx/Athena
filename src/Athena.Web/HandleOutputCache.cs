using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HandleOutputCache
    {
        private readonly AppFunc _next;

        public HandleOutputCache(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await _next(environment).ConfigureAwait(false);

            var response = environment.GetResponse();
            var statusCode = response.StatusCode;

            response.Headers.CacheControl = "no-store";

            if (statusCode >= 200 && statusCode < 300)
            {
                var result = environment.Get<EndpointExecutionResult>("endpointresults");

                if (result?.Success == true)
                {
                    var cachableResource = result?.Result as CachableResource;

                    if(cachableResource == null)
                        return;

                    var cacheData = await cachableResource.GetCacheData(environment).ConfigureAwait(false);

                    if(cacheData == null)
                        return;

                    response.Headers.CacheControl = cacheData.CacheControl;

                    if (!string.IsNullOrEmpty(cacheData.Etag))
                        response.Headers.ETag = cacheData.Etag;
                }
            }
        }
    }
}