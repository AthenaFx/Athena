using System.Collections.Generic;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Authorization
{
    public abstract class RouteAuthorizer<TRouterResult> : Authorizer where TRouterResult : RouterResult
    {
        public async Task<AuthorizationResult> IsAuthorized(IDictionary<string, object> environment, 
            AuthenticationIdentity authenticationIdentity)
        {
            var routerResult = environment.GetRouteResult();

            if(!(routerResult is TRouterResult))
                return AuthorizationResult.NotApplied;

            return await Authorize((TRouterResult) routerResult, authenticationIdentity, environment)
                .ConfigureAwait(false);
        }

        protected abstract Task<AuthorizationResult> Authorize(TRouterResult routerResult,
            AuthenticationIdentity identity, IDictionary<string, object> environment);
    }
}