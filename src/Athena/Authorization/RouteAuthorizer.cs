using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Authorization
{
    public class RouteAuthorizer<TRouterResult> : Authorizer where TRouterResult : RouterResult
    {
        private readonly 
            Func<RouterResult, AuthenticationIdentity, IDictionary<string, object>, Task<AuthorizationResult>> 
            _authorize;

        public RouteAuthorizer(
            Func<RouterResult, AuthenticationIdentity, IDictionary<string, object>, Task<AuthorizationResult>> 
                authorize)
        {
            _authorize = authorize;
        }

        public async Task<AuthorizationResult> IsAuthorized(IDictionary<string, object> environment, 
            AuthenticationIdentity authenticationIdentity)
        {
            var routerResult = environment.GetRouteResult();

            if(!(routerResult is TRouterResult))
                return AuthorizationResult.NotApplied;

            return await _authorize((TRouterResult) routerResult, authenticationIdentity, environment)
                .ConfigureAwait(false);
        }
    }
}