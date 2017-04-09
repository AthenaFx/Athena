using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web.Parsing
{
    public interface FindMediaTypesForRouterResult
    {
        Task<IReadOnlyCollection<string>> FindAvailableFor(RouterResult routerResult,
            IDictionary<string, object> environment);
    }
}