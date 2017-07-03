using System.Collections.Generic;

namespace Athena.Authorization
{
    public static class AuthorizersExtensions
    {
        public static IReadOnlyDictionary<string, string> GetDiagnosticsData(this IEnumerable<Authorizer> authorizers)
        {
            var row = 1;
            
            var result = new Dictionary<string, string>();

            foreach (var authorizer in authorizers)
            {
                result[row.ToString()] = authorizer.ToString();

                row++;
            }

            return result;
        }
    }
}