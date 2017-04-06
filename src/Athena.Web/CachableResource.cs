using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Web
{
    public interface CachableResource
    {
        Task<CacheData> GetCacheData(IDictionary<string, object> environment);
    }
}