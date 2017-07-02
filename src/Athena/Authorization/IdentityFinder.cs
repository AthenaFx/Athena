using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Authorization
{
    public interface IdentityFinder
    {
        Task<Identity> FindIdentityFor(IDictionary<string, object> environment);
    }
}