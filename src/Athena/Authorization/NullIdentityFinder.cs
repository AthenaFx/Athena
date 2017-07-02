using System.Collections.Generic;
using System.Threading.Tasks;

namespace Athena.Authorization
{
    public class NullIdentityFinder : IdentityFinder
    {
        public Task<AuthenticationIdentity> FindIdentityFor(IDictionary<string, object> environment)
        {
            return Task.FromResult(new AuthenticationIdentity(null, new Dictionary<string, string>()));
        }
    }
}