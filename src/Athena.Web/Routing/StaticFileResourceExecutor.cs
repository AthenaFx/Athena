using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Resources;
using Athena.Routing;

namespace Athena.Web.Routing
{
    public class StaticFileResourceExecutor : ResourceExecutor
    {
        public Task<ResourceExecutionResult> Execute(RouterResult resource, IDictionary<string, object> environment)
        {
            var staticFileResource = resource as StaticFileRouterResult;

            return Task.FromResult(new ResourceExecutionResult(staticFileResource != null, staticFileResource?.Read()));
        }
    }
}