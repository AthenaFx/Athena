using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Authorization
{
    public interface IdentityFinder
    {
        Task<string> FindIdentityFor(IDictionary<string, object> environment);
    }
}