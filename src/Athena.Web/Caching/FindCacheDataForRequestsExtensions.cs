using System.Collections.Generic;

namespace Athena.Web.Caching
{
    public static class FindCacheDataForRequestsExtensions
    {
        public static IReadOnlyDictionary<string, string> GetDiagnosticsData(
            this IEnumerable<FindCacheDataForRequest> cacheDataFinders)
        {
            var row = 1;
            
            var result = new Dictionary<string, string>();

            foreach (var cacheDataFinder in cacheDataFinders)
            {
                result[row.ToString()] = cacheDataFinder.ToString();

                row++;
            }

            return result;
        }
    }
}