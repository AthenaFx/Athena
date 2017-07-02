using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena
{
    public interface CheckIfResourceExists
    {
        Task<bool> Exists(RouterResult result, IDictionary<string, object> environment);
    }
}