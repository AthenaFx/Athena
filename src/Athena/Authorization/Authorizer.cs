using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Authorization
{
    public interface Authorizer
    {
        Task<bool> IsAuthorized(IDictionary<string, object> environment);
    }
}