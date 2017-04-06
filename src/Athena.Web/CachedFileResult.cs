using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Web
{
    public class CachedFileResult : CachableResource
    {
        private readonly CacheData _cacheData;

        public CachedFileResult(CacheData cacheData)
        {
            _cacheData = cacheData;
        }

        public Task<CacheData> GetCacheData(IDictionary<string, object> environment)
        {
            return Task.FromResult(_cacheData);
        }
    }
}