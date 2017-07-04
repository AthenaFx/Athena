using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Athena.Routing;

namespace Athena.Authorization
{
    public class MethodRouteAuthorizer : RouteAuthorizer<MethodResourceRouterResult>
    {
        private readonly IReadOnlyCollection<Func<MethodResourceRouterResult, AuthenticationIdentity,
            IDictionary<string, object>, Task<bool>>> _validators;

        private MethodRouteAuthorizer(IReadOnlyCollection<Func<MethodResourceRouterResult, AuthenticationIdentity, 
            IDictionary<string, object>, Task<bool>>> validators)
        {
            _validators = validators;
        }

        protected override async Task<AuthorizationResult> Authorize(MethodResourceRouterResult routerResult, 
            AuthenticationIdentity identity, IDictionary<string, object> environment)
        {
            foreach (var validator in _validators)
            {
                var valid = await validator(routerResult, identity, environment).ConfigureAwait(false);
                
                if(!valid)
                    return AuthorizationResult.Denied;
            }
            
            return AuthorizationResult.Allowed;
        }

        public static MethodRouteAuthorizerBuilder New()
        {
            return new MethodRouteAuthorizerBuilder();
        }
        
        public class MethodRouteAuthorizerBuilder
        {
            private readonly ICollection<Func<MethodResourceRouterResult, AuthenticationIdentity,
                IDictionary<string, object>, Task<bool>>> _validators =
                new List<Func<MethodResourceRouterResult, AuthenticationIdentity, IDictionary<string, object>,
                    Task<bool>>>();

            public MatchingRouteAuthorizeBuilder Routes(Func<MethodResourceRouterResult, bool> filter)
            {
                return new MatchingRouteAuthorizeBuilder(filter, this);
            }
            
            public MethodRouteAuthorizer Build()
            {
                return new MethodRouteAuthorizer(_validators.ToList());
            }
            
            public class MatchingRouteAuthorizeBuilder
            {
                private readonly Func<MethodResourceRouterResult, bool> _filter;

                private readonly MethodRouteAuthorizerBuilder _authorizerBuilder;

                public MatchingRouteAuthorizeBuilder(Func<MethodResourceRouterResult, bool> filter, 
                    MethodRouteAuthorizerBuilder authorizerBuilder)
                {
                    _filter = filter;
                    _authorizerBuilder = authorizerBuilder;
                }

                public MethodRouteAuthorizerBuilder AuthorizeWith(
                    Func<AuthenticationIdentity, IDictionary<string, object>, Task<bool>> autorize)
                {
                    _authorizerBuilder._validators.Add(async (route, identity, environment) =>
                    {
                        if (!_filter(route))
                            return true;

                        return await autorize(identity, environment).ConfigureAwait(false);
                    });

                    return _authorizerBuilder;
                }
            }
        }
    }
}