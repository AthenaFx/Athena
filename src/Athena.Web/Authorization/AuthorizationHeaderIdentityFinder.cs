using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Authorization;

namespace Athena.Web.Authorization
{
    public abstract class AuthorizationHeaderIdentityFinder : IdentityFinder
    {
        private readonly string _tokenType;

        protected AuthorizationHeaderIdentityFinder(string tokenType = "Bearer")
        {
            _tokenType = tokenType;
        }

        public async Task<AuthenticationIdentity> FindIdentityFor(IDictionary<string, object> environment)
        {
            var token = environment.GetAuthorizationToken(_tokenType);

            return await ParseToken(token, environment).ConfigureAwait(false);
        }

        protected abstract Task<AuthenticationIdentity> ParseToken(string token,
            IDictionary<string, object> environment);
    }
}