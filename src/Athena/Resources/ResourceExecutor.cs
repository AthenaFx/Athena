using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Resources
{
    public interface ResourceExecutor
    {
        Task<ResourceExecutionResult> Execute(RouterResult routerResult, IDictionary<string, object> environment);
    }
}