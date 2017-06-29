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
        private readonly AppFunc _onUnAuthorized;
        
        public Authorize(AppFunc next, IReadOnlyCollection<Authorizer> authorizers, AppFunc onUnAuthorized = null)
        {
            _next = next;
            _authorizers = authorizers;
            _onUnAuthorized = onUnAuthorized ?? (x => Task.CompletedTask);
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var unAuthorized = (await Task.WhenAll(_authorizers
                    .Select(x => x.IsAuthorized(environment)))
                .ConfigureAwait(false)).Any(x => !x);

            if (unAuthorized)
            {
                await _onUnAuthorized(environment).ConfigureAwait(false);
                
                return;
            }

            await _next(environment);
        }
    }
}