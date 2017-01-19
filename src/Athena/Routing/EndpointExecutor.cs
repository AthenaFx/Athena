using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Routing
{
    public interface EndpointExecutor
    {
        Task<EndpointExecutionResult> Execute(object endpoint, IDictionary<string, object> environment);
    }
}