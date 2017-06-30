using System.Collections.Generic;
using Athena.Configuration;

namespace Athena
{
    public static class ContextExtensions
    {
        public const string AthenaContextKey = "athenacontext";

        public static AthenaContext GetAthenaContext(this IDictionary<string, object> environment)
        {
            return environment.Get<AthenaContext>(AthenaContextKey);
        }
    }
}