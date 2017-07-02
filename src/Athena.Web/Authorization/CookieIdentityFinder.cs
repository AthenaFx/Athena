using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Authorization;

namespace Athena.Web.Authorization
{
    public class CookieIdentityFinder : IdentityFinder
    {
        private readonly string _cookieName;
        private readonly Func<string, IDictionary<string, object>, Task<Identity>> _findIdentityFromCookieValue;
        
        public CookieIdentityFinder(string cookieName, 
            Func<string, IDictionary<string, object>, Task<Identity>> findIdentityFromCookieValue)
        {
            _cookieName = cookieName;
            _findIdentityFromCookieValue = findIdentityFromCookieValue;
        }

        public async Task<Identity> FindIdentityFor(IDictionary<string, object> environment)
        {
            var cookie = environment.GetRequest().Cookies[_cookieName];

            if (string.IsNullOrEmpty(cookie))
                return null;

            return await _findIdentityFromCookieValue(cookie, environment).ConfigureAwait(false);
        }
    }
}