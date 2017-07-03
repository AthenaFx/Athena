using System.Collections.Generic;

namespace Athena
{
    public static class CheckIfResourceExistsExtensions
    {
        public static IReadOnlyDictionary<string, string> GetDiagnosticsData(
            this IEnumerable<CheckIfResourceExists> routeCheckers)
        {
            var row = 1;
            
            var result = new Dictionary<string, string>();

            foreach (var routeChecker in routeCheckers)
            {
                result[row.ToString()] = routeChecker.ToString();

                row++;
            }

            return result;
        }

    }
}