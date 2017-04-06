using System.Collections.Generic;
using System.Reflection;

namespace Athena.Web.Routing
{
    public class Route
    {
        public Route(string pattern, MethodInfo destination, IReadOnlyCollection<string> availableHttpMethods)
        {
            Pattern = pattern;
            Destination = destination;
            AvailableHttpMethods = availableHttpMethods;
        }

        public string Pattern { get; }
        public MethodInfo Destination { get; }
        public IReadOnlyCollection<string> AvailableHttpMethods { get; }
    }
}