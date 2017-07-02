using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena
{
    public class CheckIfRouteExists : CheckIfResourceExists
    {
        public Task<bool> Exists(RouterResult result, IDictionary<string, object> environment)
        {
            return Task.FromResult(result != null);
        }
    }
}