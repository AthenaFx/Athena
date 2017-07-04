using System.Collections.Generic;

namespace Athena.Web.Authorization
{
    public static class AuthorizationEnvironmentExtensions
    {
        public static string GetAuthorizationToken(this IDictionary<string, object> environment,
            string tokenType = "Bearer")
        {
            var authorization = environment.GetRequest().Headers.GetHeader("Authorization");

            if (string.IsNullOrEmpty(authorization))
                return null;

            var parts = authorization.Split(new[]{' '}, 2);

            if (parts.Length < 2)
                return null;

            if (parts[0] != tokenType)
                return null;

            return parts[1];
        }
    }
}