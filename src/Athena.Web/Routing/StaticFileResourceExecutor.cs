using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Resources;

namespace Athena.Web.Routing
{
    public class StaticFileResourceExecutor : ResourceExecutor<StaticFileRouterResult>
    {
        public Task<object> Execute(StaticFileRouterResult resource, IDictionary<string, object> environment)
        {
            return Task.FromResult<object>(resource.Read());
        }
    }
}