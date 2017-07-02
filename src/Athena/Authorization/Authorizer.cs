using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Authorization
{
    public interface Authorizer
    {
        Task<AuthorizationResult> IsAuthorized(IDictionary<string, object> environment, string identity);
    }
}