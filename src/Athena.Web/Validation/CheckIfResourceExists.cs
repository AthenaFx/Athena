using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Web.Validation
{
    public interface CheckIfResourceExists
    {
        Task<bool> Exists(RouterResult result, IDictionary<string, object> environment);
    }
}