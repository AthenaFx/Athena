using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Web.Caching
{
    public interface FindCacheDataForRequest
    {
        Task<CacheData> Find(IDictionary<string, object> environment);
    }
}