using System.Collections.Generic;

namespace Athena.Web.Validation
{
    public static class ValidateRouteResultsExtensions
    {
        public static IReadOnlyDictionary<string, string> GetDiagnosticsData(
            this IEnumerable<ValidateRouteResult> validateRouteResults)
        {
            var row = 1;
            
            var result = new Dictionary<string, string>();

            foreach (var validateRouteResult in validateRouteResults)
            {
                result[row.ToString()] = validateRouteResult.ToString();

                row++;
            }

            return result;
        }
    }
}