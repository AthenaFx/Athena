using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Authorization;

namespace Athena.Web.Authorization
{
    public class CookieIdentityFinder : IdentityFinder
    {
        private readonly string _cookieName;
        private readonly Func<string, IDictionary<string, object>, Task<AuthenticationIdentity>> _findIdentityFromCookieValue;
        
        public CookieIdentityFinder(string cookieName, 
            Func<string, IDictionary<string, object>, Task<AuthenticationIdentity>> findIdentityFromCookieValue)
        {
            _cookieName = cookieName;
            _findIdentityFromCookieValue = findIdentityFromCookieValue;
        }

        public async Task<AuthenticationIdentity> FindIdentityFor(IDictionary<string, object> environment)
        {
            var cookie = environment.GetRequest().Cookies[_cookieName];

            if (string.IsNullOrEmpty(cookie))
                return null;

            return await _findIdentityFromCookieValue(cookie, environment).ConfigureAwait(false);
        }
    }
}