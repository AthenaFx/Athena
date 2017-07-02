using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Athena.Authorization
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class Authorize
    {
        private readonly AppFunc _next;
        private readonly IReadOnlyCollection<Authorizer> _authorizers;
        private readonly IdentityFinder _identityFinder;
        private readonly AppFunc _onUnAuthorized;
        
        public Authorize(AppFunc next, IReadOnlyCollection<Authorizer> authorizers, IdentityFinder identityFinder, 
            AppFunc onUnAuthorized = null)
        {
            _next = next;
            _authorizers = authorizers;
            _identityFinder = identityFinder;
            _onUnAuthorized = onUnAuthorized ?? (x => Task.CompletedTask);
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var identity = await _identityFinder.FindIdentityFor(environment).ConfigureAwait(false);
            
            var unAuthorized = (await Task.WhenAll(_authorizers
                    .Select(x => x.IsAuthorized(environment, identity)))
                .ConfigureAwait(false))
                .Contains(AuthorizationResult.Denied);

            if (unAuthorized)
            {
                await _onUnAuthorized(environment).ConfigureAwait(false);
                
                return;
            }

            await _next(environment);
        }
    }
}