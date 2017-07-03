using System.Collections.Generic;
using System.Linq;

namespace Athena.Web.Parsing
{
    public static class ResultParsersExtensions
    {
        public static IReadOnlyDictionary<string, string> GetDiagnosticsData(this IEnumerable<ResultParser> resultParsers)
        {
            return resultParsers
                .SelectMany(x => x.MatchingMediaTypes.Select(y => new
                {
                    MediaType = y,
                    Parser = x
                })).GroupBy(x => x.MediaType)
                .ToDictionary(x => x.Key, 
                    x => $"{string.Join(", ", x.Select(y => y.Parser.ToString()).Distinct())}");
        }
    }
}