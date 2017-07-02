using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;
using Athena.Web.Routing;

namespace Athena.Web.Parsing
{
    public class FindAvailableMediaTypesFromStaticFileRouteResult : FindMediaTypesForRequest
    {
        public Task<IReadOnlyCollection<string>> FindAvailableFor(IDictionary<string, object> environment)
        {
            var staticFileResult = environment.GetRouteResult() as StaticFileRouterResult;

            if(staticFileResult == null)
                return Task.FromResult<IReadOnlyCollection<string>>(new List<string>());

            return Task.FromResult<IReadOnlyCollection<string>>(new List<string>
            {
                staticFileResult.ContentType
            });
        }
    }
}