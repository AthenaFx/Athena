using System.Collections.Generic;

namespace Athena.Resources
{
    public static class ResourceExecutorExtensions
    {
        public static IReadOnlyDictionary<string, string> GetDiagnosticsData(
            this IEnumerable<ResourceExecutor> resourceExecutors)
        {
            var row = 1;
            
            var result = new Dictionary<string, string>();

            foreach (var resourceExecutor in resourceExecutors)
            {
                result[row.ToString()] = resourceExecutor.ToString();

                row++;
            }

            return result;
        }
    }
}