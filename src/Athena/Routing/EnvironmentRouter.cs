using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Routing
{
    public interface EnvironmentRouter
    {
        Task<RouterResult> Route(IDictionary<string, object> environment);
    }
}