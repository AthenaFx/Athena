using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;
using Athena.Web.Routing;

namespace Athena.Web.Parsing
{
    public class FindAvailableMediaTypesFromStaticFileRouteResult : FindMediaTypesForRouterResult
    {
        public Task<IReadOnlyCollection<string>> FindAvailableFor(RouterResult routerResult, IDictionary<string, object> environment)
        {
            var staticFileResult = routerResult as StaticFileRouterResult;

            if(staticFileResult == null)
                return Task.FromResult<IReadOnlyCollection<string>>(new List<string>());

            return Task.FromResult<IReadOnlyCollection<string>>(new List<string>
            {
                staticFileResult.ContentType
            });
        }
    }
}