using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Authorization;

namespace Athena.Web.Authorization
{
    public class AuthorizationHeaderIdentityFinder : IdentityFinder
    {
        private readonly IReadOnlyDictionary<string, Func<string, IDictionary<string, object>, Task<AuthenticationIdentity>>> 
            _identityFinderTypes;

        public AuthorizationHeaderIdentityFinder(
            IReadOnlyDictionary<string, Func<string, IDictionary<string, object>, Task<AuthenticationIdentity>>> identityFinderTypes)
        {
            _identityFinderTypes = identityFinderTypes;
        }

        public async Task<AuthenticationIdentity> FindIdentityFor(IDictionary<string, object> environment)
        {
            var authorization = environment.GetRequest().Headers.GetHeader("Authorization");

            if (string.IsNullOrEmpty(authorization))
                return null;

            var parts = authorization.Split(new[]{' '}, 2);

            if (parts.Length < 2)
                return null;

            if (!_identityFinderTypes.ContainsKey(parts[0]))
                return null;

            return await _identityFinderTypes[parts[0]](parts[1], environment).ConfigureAwait(false);
        }
    }
}