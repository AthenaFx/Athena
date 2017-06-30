using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Athena.Web.Routing
{
    public class DefaultRoutePatternMatcher : RoutePatternMatcher
    {
        private static readonly ConcurrentDictionary<string, Regex> MatcherCache = new ConcurrentDictionary<string, Regex>();

        public RouteMatchResult Match(string requestedPath, string routePath)
        {
            var routePathPattern =
                MatcherCache.GetOrAdd(routePath, s => BuildRegexMatcher(routePath));

            requestedPath =
                TrimTrailingSlashFromRequestedPath(requestedPath);

            var match =
                routePathPattern.Match(requestedPath);

            return new RouteMatchResult(
                match.Success,
                GetParameters(routePathPattern, match.Groups));
        }

        private static string TrimTrailingSlashFromRequestedPath(string requestedPath)
        {
            if (!requestedPath.Equals("/"))
            {
                requestedPath = requestedPath.TrimEnd('/');
            }

            return requestedPath;
        }

        private static Regex BuildRegexMatcher(string path)
        {
            var segments =
                path.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

            var parameterizedSegments =
                GetParameterizedSegments(segments);

            var pattern =
                string.Concat(@"^/", string.Join("/", parameterizedSegments), @"$");

            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private static IReadOnlyDictionary<string, object> GetParameters(Regex regex, GroupCollection groups)
        {
            var data = new Dictionary<string, object>();

            for (var i = 1; i <= groups.Count; i++)
            {
                data[regex.GroupNameFromNumber(i)] = groups[i].Value;
            }

            return data;
        }

        private static IEnumerable<string> GetParameterizedSegments(IEnumerable<string> segments)
        {
            foreach (var segment in segments)
            {
                var current = segment;

                if (current.IsParameterized())
                {
                    var replacement =
                        string.Format(CultureInfo.InvariantCulture, @"(?<{0}>([^\/]+))", segment.GetParameterName());

                    current = segment.Replace(segment, replacement);
                }

                yield return current;
            }
        }
    }
}